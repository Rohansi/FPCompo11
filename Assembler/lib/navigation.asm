
; int getSpeedX()
getSpeedX:
    push r7
    push r8

    xor r7, r7
    int DEV_NAVIGATION
    mov r0, r7

    pop r8
    pop r7
    ret

; int getSpeedY()
getSpeedY:
    push r7
    push r8

    xor r7, r7
    int DEV_NAVIGATION
    mov r0, r8

    pop r8
    pop r7
    ret

; int getSpeedTurn()
getSpeedTurn:
    push r7

    mov r7, 1
    int DEV_NAVIGATION
    mov r0, r7

    pop r7
    ret

; int getHeading()
getHeading:
    push r7

    mov r7, 2
    int DEV_NAVIGATION
    mov r0, r7

    pop r7
    ret
