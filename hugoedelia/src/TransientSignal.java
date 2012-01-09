
import java.util.LinkedList;
import java.util.Queue;

/**
 * Created by IntelliJ IDEA.
 * User: ferreirah
 * Date: 05-12-2011
 * Time: 21:06
 * To change this template use File | Settings | File Templates.
 */

public class TransientSignal {

        private final Object _monitor = new Object();
        private static Queue<locked> _requests;

        private static class locked {
            public boolean isSignaled;
        }

        public TransientSignal() {
            _requests = new LinkedList<locked>();
        }

        public boolean await() throws InterruptedException {
            synchronized ( _monitor ) {
                final locked request = new locked();
                request.isSignaled = false;
                _requests.add( request );
                try {
                    while ( true ) {
                        if ( request.isSignaled ) {
                            _requests.remove( request );
                            return true;
                        }
                        _monitor.wait();
                    }
                } catch ( InterruptedException e ) {
                    Thread.currentThread().interrupt();
                    return false;
                }
            }

        }

        public void signalOne() {
            synchronized ( _monitor ) {
                if ( _requests.isEmpty() ) {
                    return;
                }
                final locked request = _requests.remove();
                request.isSignaled = true;
                System.out.println("Thread Unlocked");
                _monitor.notifyAll();
            }
        }

        public void signalAll() {
            synchronized ( _monitor ) {
                if ( _requests.isEmpty() ) {
                    return;
                }
                for ( final locked request : _requests ) {
                    request.isSignaled = true;
                    System.out.println("Threads Unlocked");
                }
                _requests = new LinkedList<locked>();
                _monitor.notifyAll();
            }
        }
    }

