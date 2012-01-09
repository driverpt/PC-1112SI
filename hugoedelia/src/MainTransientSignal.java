import java.io.Console;
import java.lang.reflect.Array;

/**
 * Created by IntelliJ IDEA.
 * User: ferreirah
 * Date: 05-12-2011
 * Time: 21:12
 * To change this template use File | Settings | File Templates.
 */
public class MainTransientSignal {
    public static void main(String[] args){
        final TransientSignal tr = new TransientSignal();
        int j = 5;
        Thread[] arr = new Thread[j];
        for (int i=0;i<j;i++){
            arr[i]=new Thread(){
                @Override
                public void run(){
                    try {
                        tr.await();
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    }
                }
            };
            arr[i].start();
            System.out.println("Thread" + i +  "Started");
        }

        Thread unlockOne = new Thread(){
            @Override
            public void run(){
                    tr.signalOne();
            }
        };
        unlockOne.start();
        System.out.println("unlockOne Thread Started");

        Thread unlockAll = new Thread(){
            @Override
            public void run(){
                    tr.signalAll();
            }
        };
        unlockAll.start();
        System.out.println("unlockAll Thread Started");

    }
}
