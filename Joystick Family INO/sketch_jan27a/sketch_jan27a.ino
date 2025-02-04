#include <SoftwareSerial.h>   // Incluimos la librería  SoftwareSerial  
SoftwareSerial BT(18,19);    // Definimos los pines RX y TX del Arduino conectados al Bluetooth
 
//#define BT Serial

#define SEGA_J1_UP 2
#define SEGA_J1_DOWN 3
#define SEGA_J1_LEFT 4
#define SEGA_J1_RIGHT 5
#define SEGA_J1_A 6
#define SEGA_J1_B 7
#define SEGA_J1_PAUSE 8
#define SEGA_J1_MODE 9
#define SEGA_J1_C 10
#define SEGA_J1_X 11
#define SEGA_J1_Y 12
#define SEGA_J1_Z 13

#define FAM_J1_UP 2
#define FAM_J1_DOWN 3
#define FAM_J1_LEFT 4
#define FAM_J1_RIGHT 5
#define FAM_J1_A 6
#define FAM_J1_B 7
#define FAM_J1_PAUSE 8 
#define FAM_J1_MODE 9

#define FAM_J2_UP 10
#define FAM_J2_DOWN 11
#define FAM_J2_LEFT 12
#define FAM_J2_RIGHT 13
#define FAM_J2_A 14
#define FAM_J2_B 15
#define FAM_J2_PAUSE 16
#define FAM_J2_MODE 17

#define ON LOW
#define OFF HIGH

#define ZEROCHAR '0'

void PrintNumber(int n)
{
    BT.write(n + ZEROCHAR);
}

void SendBinary(int input, int bitStart, int bitEnd)
{
    for(int i=bitEnd; i >= bitStart; i--)
    {
        int state = (input & (1 << i)) > 0;
        PrintNumber(state);
    }
}

void SetOutput(int input, int from, int to)
{
  for(int i=from; i<= to; i++)
  {
      int current = 1 << (i-from); 
      int state =  OFF;
      if(input & current)
          state = ON;
      
      digitalWrite(i, state);
  }
  //BT.write('\n');

}

void setup() 
{
    //Setup BT input
    BT.begin(9600);

    //Setup output for Family
    for(int i = FAM_J1_UP; i <= FAM_J2_MODE; i++)
    {
        pinMode(i, OUTPUT);
        digitalWrite(i, OFF);// turn the button OFF (HIGH is the voltage level)
    }
}

bool handshaked = false;

void Handshake(int input){
    if(input == 'h'){
        handshaked = true;
        BT.write('o');
    }
    else{
        BT.write(input);
    }
}

void loop() 
{
    if(!handshaked)
    {
        while(BT.available() == 0);
        int input = BT.read();
        while(BT.available() == 0);
        input = BT.read();
        Handshake(input);
        return;
    }
    int FAM_JOY_1 = 256;
    int FAM_JOY_2 = 0x200;
    //Read input
    if(BT.available() > 0)
    {
        int input = BT.read();
        
        while(BT.available() == 0);

        int joy = BT.read();

        if(input != 0)
        {
            //SendBinary(input, 0, 7);
            //decrease inceased value in order to avoid 0 input
            input -= 1; 
            if(joy == 1)
            {
                SetOutput(input, FAM_J1_UP, FAM_J1_MODE);
                //BT.write("o1");
            }
            else if(joy == 2)
            {
                SetOutput(input, FAM_J2_UP, FAM_J2_MODE);
                //BT.write("o2");
            }
            else if(joy == 'h'){
                Handshake(input+ 1);
            }
            else
            {
                BT.write("er");
                BT.write(joy);
                while(BT.available() == 0);
                //clean buffer
                BT.read();
                BT.write("cl");
            }
        }
        
    }
}
