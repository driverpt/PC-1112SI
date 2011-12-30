package org.isel.deetc.leic.pc;

import java.util.concurrent.BrokenBarrierException;
import java.util.concurrent.CyclicBarrier;

import org.apache.log4j.BasicConfigurator;
import org.apache.log4j.Level;
import org.apache.log4j.Logger;
import org.junit.Before;
import org.junit.Test;

import junit.framework.TestCase;

public class PhasedGateTests extends TestCase {

    private static final int           MAX_PARTICIPANTS = 8;
    
    private static final CyclicBarrier barrier          = new CyclicBarrier( MAX_PARTICIPANTS );

    private static final Logger logger = Logger.getLogger( PhasedGateTests.class ); 
    
    static {
        BasicConfigurator.configure();    
    }
    
    @Before
    public void startUp() {
        logger.setLevel( Level.INFO );
    }
    
    @Test( expected = IllegalStateException.class )
    public void testIfMaxParticipantsWorks() {
        final PhasedGate phasedGate = new PhasedGate( MAX_PARTICIPANTS );

        barrier.reset();
        for ( int i = 0; i < MAX_PARTICIPANTS; ++i ) {
            new PhasedGateThread( i + 1 ) {
                @Override
                public void run() {
                    try {
                        logger.debug( "arriving at barrier" );
                        barrier.await();
                        logger.debug( "barrier released" );
                        logger.debug( "arriving at phased gate" );
                        phasedGate.await();
                        logger.debug( "left phased gate" );
                    } catch ( final InterruptedException e ) {
                        logger.debug( "Interrupted" );
                    } catch ( final BrokenBarrierException e ) {
                        logger.debug( "Broken Barrier" );
                    }
                }
            }.start();
        }
        try {
            Thread.sleep( 2000 );
            phasedGate.await();
        } catch ( InterruptedException e ) {
            logger.fatal( e.getMessage(), e );
        }
    }

    private static class PhasedGateThread extends Thread {
        public final int _threadNumber;

        public PhasedGateThread( int threadNumber ) {
            _threadNumber = threadNumber;
        }
    }
}
