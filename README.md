# Kunkin KP184 command line client
Remote control command line client for Kunkin KP184 electronic load.

Example execution:
kp184 port com11; baud 115200; address 1; get_status

```
KP184 Controller

Usage: kp184 command [params]; command [params]; etc

Control the KP184, specify port, baud, address before other commands

Commands:

port        com_port                                              Set the com port
baud        baud_rate(int32)                                      Set the baud rate
address     device_address(byte)                                  Set the device address
switch      [off|on]                                              Switch the load on or off
on                                                                Switch the load on
off                                                               Switch the load off
delay       ms(int32)                                             Do nothing for N milliseconds
mode        [cv|cc|cr|cw]                                         Set the load mode
checksum    enable(boolean)                                       true/false - enable/disable the checksum check
ramp        from(int32) to(int32) step(int32) interval_ms(int32)  Do a ramp
current     current(uint32)                                       Set the current in milliamps
power       watts(uint32)                                         Set the watts in deci Watts (0.01W)
resistance  ohms(uint32)                                          Set the resistance in 0.1 Ohm
get_status                                                        Returns the status of KP184
get_switch                                                        Returns if device is On (=1) or Off (=0)
get_mode                                                          Returns the mode (cv=0, cc=1, cr=2, cw=3)
get_current                                                       Reads the current in mA
get_voltage                                                       Reads the voltage in mV
verbose                                                           Set verbose mode
loglevel    [debug|verbose|info|warning|error]                    Set the log level
help                                                              Show this help text

```

.NET 4.7.2 is required

Note: Does **not work in Windows PowerShell**, use cmd.exe instead
