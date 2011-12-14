package org.isel.deetc.leic.pc;

import java.util.LinkedList;
import java.util.Queue;

public class TransientSignal {

  private final Object  _monitor = new Object();
  private Queue<Waiter> _requests;

  public TransientSignal() {
    _requests = new LinkedList<Waiter>();
  }

  public boolean await() throws InterruptedException {
    synchronized (_monitor) {
      final Waiter request = new Waiter();
      request.isSignaled = false;
      _requests.add(request);
      try {
        while (true) {
          if (request.isSignaled) {
            _requests.remove(request);
            return true;
          }
          _monitor.wait();
        }
      } catch (InterruptedException e) {
        _requests.remove(request);
        Thread.currentThread().interrupt();
        return false;
      }
    }

  }

  public void signalOne() {
    synchronized (_monitor) {
      if (_requests.isEmpty()) {
        return;
      }
      final Waiter request = _requests.remove();
      request.isSignaled = true;
      _monitor.notifyAll();
    }
  }

  public void signalAll() {
    synchronized (_monitor) {
      if (_requests.isEmpty()) {
        return;
      }
      for (final Waiter request : _requests) {
        request.isSignaled = true;
      }
      _requests.clear();
      _monitor.notifyAll();
    }
  }

  private static class Waiter {
    public boolean isSignaled;
  }
}
