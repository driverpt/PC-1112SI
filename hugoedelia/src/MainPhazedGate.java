import java.io.Console;

/**
 * Created by IntelliJ IDEA.
 * User: ferreirah
 * Date: 05-12-2011
 * Time: 21:12
 * To change this template use File | Settings | File Templates.
 */
public class MainPhazedGate {
    public static void main(String[] args){
        final PhasedGate thr = new PhasedGate(5);
        for (int i=0;i<5;i++){
            new Thread(){
                @Override
                public void run(){
                    try {
                        thr.await();
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    }
                }
            }.start();
            System.out.println("Thread" + i +  "Started");
        }

        Thread th3 = new Thread(){
            @Override
            public void run(){
                try {
                    thr.await();
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
            }
        };
        th3.start();
        System.out.println("Last Thread Started");
    }
}
