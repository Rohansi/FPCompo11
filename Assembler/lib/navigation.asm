
; int getCoordX()
getCoordX:
    push r1
    xor r0, r0
    int DEV_NAVIGATION
    pop r1
    ret

; int getCoordY()
getCoordY:
    push r1
    xor r0, r0
    int DEV_NAVIGATION
    mov r0, r1
    pop r1
    ret

; int getSpeedX()
getSpeedX:
    push r1
    mov r0, 1
    int DEV_NAVIGATION
    pop r1
    ret

; int getSpeedY()
getSpeedY:
    push r1
    mov r0, 1
    int DEV_NAVIGATION
    mov r0, r1
    pop r1
    ret

; int getSpeedTurn()
getSpeedTurn:
    mov r0, 2
    int DEV_NAVIGATION
    ret

; int getHeading()
getHeading:
    mov r0, 3
    int DEV_NAVIGATION
    ret
