#include <stdio.h>
#include <Windows.h>

volatile BOOLEAN CountFlag;

static UINT  SwitchCount;

DWORD WINAPI YieldingThread( LPVOID lpParam ) {
  while( CountFlag ) {
    ++SwitchCount;
    SwitchToThread();
  }
  return 0;
}

DWORD WINAPI AwakenThread( LPVOID lpParam ) {
  while( CountFlag ) {
    ++SwitchCount;
    SwitchToThread();
  }
  return 0;
}

int main( int argc, const char ** argv ) {
  HANDLE awakeThread, yieldThread;
  UINT X;
  LONGLONG startTickCount, endTickCount;
  SetProcessAffinityMask( GetCurrentProcess(), 1L );
  
  CountFlag = TRUE;
  
  printf( "Press Enter to Start\n" );
  getchar();
  
  yieldThread = CreateThread( NULL
                            , 0
                            , YieldingThread
                            , 0
                            , CREATE_SUSPENDED
                            , NULL
                            );
  awakeThread = CreateThread( NULL
                            , 0
                            , AwakenThread
                            , 0
                            , CREATE_SUSPENDED
                            , NULL
                            );
                            
  ResumeThread( yieldThread );
  ResumeThread( awakeThread );
  
  startTickCount = __rdtsc();
  getchar();
  endTickCount = __rdtsc();
  CountFlag = FALSE;
  X = endTickCount - startTickCount;
  printf( "Total Ticks => %u\n", X ) ;
  printf( "Switch Count => %u\n", SwitchCount ) ;
  printf( "Average Thread Context Switching Time => %d\n", (X/ SwitchCount));
  getchar();
  return 0;
}