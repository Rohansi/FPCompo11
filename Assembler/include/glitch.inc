include 'loonyvm.inc'

VAR_GENERAL         = 0
VAR_RADARVALUE      = 1
VAR_SPEED           = 2

DEV_SYSTEM          =  0
DEV_TIMER           =  1
DEV_DEBUG           =  3
DEV_NAVIGATION      = 10
DEV_RADAR           = 11
DEV_ENGINES         = 12
DEV_GUNS            = 13
DEV_BROADCAST       = 14
DEV_SYSCALL         = 15

RADAR_ASTEROID      = 0
RADAR_BULLET        = 1
RADAR_ALLY          = 2
RADAR_ENEMY         = 3

RADAR_RAYCOUNT      = 126
RADAR_INVALIDRAY    = RADAR_RAYCOUNT + 1
RADAR_INVALIDTYPE   = 127
RADAR_INVALIDDIST   = 0x7FFF
