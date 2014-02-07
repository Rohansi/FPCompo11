
; void setThrustSpeed(int speed)
setThrustSpeed:
    push r0
    push r1
    xor r0, r0
    mov r1, [sp + 12]
    int DEV_ENGINES
    pop r1
    pop r0
    retn 4

; void setTurnSpeed(int speed)
setTurnSpeed:
    push r0
    push r1
    mov r0, 1
    mov r1, [sp + 12]
    int DEV_ENGINES
    pop r1
    pop r0
    retn 4
