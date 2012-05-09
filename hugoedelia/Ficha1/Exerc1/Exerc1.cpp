// Exerc1.cpp : Defines the entry point for the console application.
//

#include<windows.h>
#include <stdio.h>

#define NR_THREADS 2

LARGE_INTEGER start,stop;
LARGE_INTEGER frequency;
volatile BOOL Running = TRUE;
LONGLONG		startTcCount, endTcCount, TcCount, dif;


#pragma intrinsic(__rdtsc)


DWORD WINAPI threadProcess(LPVOID arg)
{		
	//comutação de uma das duas threads
	while(Running)
	{		
		++TcCount;
		SwitchToThread();	
		
	}
	return 0;
}	

void switchThreadTime()
{
	HANDLE th_Handles[NR_THREADS];
	DWORD thID[NR_THREADS];	
	int i=0;	

		
	//Create threads
	for(i;i<NR_THREADS;i++){
		thID[i]=i;
		th_Handles[i]= CreateThread(NULL,0,threadProcess,(LPVOID)&thID[i],CREATE_SUSPENDED,NULL);
	}

	//set start Time
	QueryPerformanceCounter(&start);	
	startTcCount += __rdtsc();

	for(i=0;i<NR_THREADS;i++)
	{
		ResumeThread(th_Handles[i]);
	}

	printf("enter to terminate\n");
	getchar();
	Running=FALSE;

	//set stop Time	
	QueryPerformanceCounter(&stop);
	endTcCount += __rdtsc();

	dif = ( (stop.QuadPart - start.QuadPart) * 1000000000LL )/frequency.QuadPart;
		

	printf("Switch thread time >%I64d tcs\n", (endTcCount - startTcCount)/TcCount);
	//context switch time in nanos
	printf("Switch thread time  -> %I64d ns",dif/TcCount);
			
}


void switchThreadWin()
{
	Running=TRUE;
	//Sets the primary thread priority to highest to minimize preemption effects
	SetThreadPriority(GetCurrentThread(),THREAD_PRIORITY_HIGHEST);

	//sets to use of only one processor
	SetProcessAffinityMask(GetCurrentProcess(),1);

	//set frequency
	QueryPerformanceFrequency(&frequency);	

	switchThreadTime();	
	
	

}

VOID main()
{
	printf("\n Switch Thread Win \n");
	//metodo que invoca duas threads e contabiliza o switch
	switchThreadWin();

	
	getchar();

	return ;
}

