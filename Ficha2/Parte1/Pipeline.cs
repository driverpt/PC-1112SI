using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parte1
{
    public class Pipeline<TInput, TOutput>
    {
        private readonly Func<TInput, TOutput> _currentStage;

        protected Pipeline()
        {
        }

        public Pipeline( Func<TInput, TOutput> stage ) : this()
        {
            _currentStage = stage;
        }

        /* Retorna um novo Pipeline ao qual foi acrescentado um passo que converte
        elementos do tipo TOutput em elementos do tipo TNextOutput */
        public Pipeline<TInput, TNextOutput> Next<TNextOutput>( Func<TOutput, TNextOutput> nextStage )
        {
            return new PipelineExtension<TNextOutput>( this, nextStage );
        }

        /* executa o pipeline com possibilidade de cancelamento. O input é uma
        enumeração de elementos do tipo do primeiro passo e o output uma enumeração
        de elementos do tipo do último passo. */
        public IEnumerable<TOutput> Run( IEnumerable<TInput> source, CancellationToken token )
        {
            var outputBuffer = new BlockingCollection<TOutput>();
            ++Program.NumberOfTasks;
            var task = Task.Factory.StartNew( () => DoFillBuffer( source, outputBuffer ), token, TaskCreationOptions.None, TaskScheduler.Default );
            
            foreach ( var outputElement in outputBuffer.GetConsumingEnumerable( token ) )
            {
                yield return outputElement;
            }
            task.Wait();
        }

        protected virtual void DoFillBuffer( IEnumerable<TInput> source, BlockingCollection<TOutput> outputBuffer )
        {
            foreach (var outputElement in source.Select(inputElement => _currentStage(inputElement)))
            {
                outputBuffer.Add(outputElement);
            }
            outputBuffer.CompleteAdding();
        }

        /* executa o pipeline sem possibilidade de cancelamento. */
        public IEnumerable<TOutput> Run( IEnumerable<TInput> source )
        {
            return Run( source, new CancellationToken() );
        }

        private class PipelineExtension<TNextOutput> : Pipeline<TInput, TNextOutput>
        {
            private readonly Pipeline<TInput, TOutput> _previousPipeline;
            private readonly Func<TOutput, TNextOutput> _lastStage; 

            public PipelineExtension(Pipeline<TInput, TOutput> previousStagePipeline, Func<TOutput, TNextOutput> lastStage)
            {
                _previousPipeline = previousStagePipeline;
                _lastStage = lastStage;
            }

            protected override void DoFillBuffer( IEnumerable<TInput> source, BlockingCollection<TNextOutput> outputBuffer )
            {
                foreach( var element in _previousPipeline.Run(source))
                {
                    outputBuffer.Add(_lastStage(element));
                }
                outputBuffer.CompleteAdding();
            }
        }
    }
}