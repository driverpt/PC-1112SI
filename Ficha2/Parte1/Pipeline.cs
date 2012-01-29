using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            var task = Task.Factory.StartNew( () => DoFillBuffer( source, outputBuffer ), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default );
            task.Wait();
            outputBuffer.CompleteAdding();
            return outputBuffer.GetConsumingEnumerable( token );
        }

        protected virtual void DoFillBuffer( IEnumerable<TInput> source, BlockingCollection<TOutput> outputBuffer )
        {
            foreach( TInput inputElement in source )
            {
                var outputElement = _currentStage(inputElement);
                outputBuffer.Add(outputElement);
            }
        }

        /* executa o pipeline sem possibilidade de cancelamento. */
        public IEnumerable<TOutput> Run( IEnumerable<TInput> _source )
        {
            return Run( _source, new CancellationToken(false) );
        }

        private class PipelineExtension<TNextOutput> : Pipeline<TInput, TNextOutput>
        {
            private Pipeline<TInput, TOutput> _previousPipeline;
            private Func<TOutput, TNextOutput> _lastStage; 

            public PipelineExtension(Pipeline<TInput, TOutput> previousStagePipeline, Func<TOutput, TNextOutput> lastStage)
            {
                _previousPipeline = previousStagePipeline;
                _lastStage = lastStage;
            }

            protected override void DoFillBuffer( IEnumerable<TInput> source, BlockingCollection<TNextOutput> outputBuffer )
            {
                Parallel.ForEach(_previousPipeline.Run(source, new CancellationToken(false)), (element) => outputBuffer.Add(_lastStage(element)));
            }
        }
    }
}