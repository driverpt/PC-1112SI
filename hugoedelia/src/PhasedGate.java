/**
 * Created by IntelliJ IDEA.
 * User: ferreirah
 * Date: 05-12-2011
 * Time: 20:31
 * To change this template use File | Settings | File Templates.
 */
public class PhasedGate {
    private int _participant;
    private int _arrived;
    private final Object _lock;
    private boolean _isSignaled=false;

    public PhasedGate(int participants){
        _participant = participants;
        _arrived = 0;
        _lock = new Object();
    }

    public void await() throws InterruptedException {
        synchronized (_lock){
            if(_arrived>_participant){
                System.out.println("Phazed Gate - Donkey");
                throw new IllegalStateException();
            }
        _arrived++;
            if(_arrived == _participant){
                System.out.println("Phazed Gate Open");
                _isSignaled = true;
                _arrived++;
                _lock.notifyAll();
                System.out.println("Phazed Gate - Thread Exit");
            }
            try{
            while(true){
                System.out.println("Phazed Gate - Me is waiting....");
                _lock.wait();
                if(_isSignaled){
                    System.out.println("Phazed Gate - Thread Exit");
                    return;
                }
            }
            }catch (InterruptedException ie){
                System.out.println("Phazed Gate - 1 Thread Interrupted");
                RemoveParticipant();
                throw ie;
            }
        }
    }

    public void RemoveParticipant(){
        synchronized (_lock){
            if ( _arrived>_participant ) {
                System.out.println("Phazed Gate - Donkey");
                throw new IllegalStateException();
            }
            _arrived--;
        }
    }
}
