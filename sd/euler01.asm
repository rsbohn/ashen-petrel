    ORG 200
START:
    LOAD MAX
    STAX
    ZERO
LOOP:
    INCA
    DUP
    LDI 3
    DIV, DELB
    TEST
    DEL
    BE SUM
    DUP
    LDI 5
    DIV, DELB
    TEST
    DEL
    BE SUM
    DXBZ P+2
    BR LOOP
    HALT

SUM:                ; (n -- n) X is destroyed
    DUP             ;TOTAL += n
    STAX
    LOAD TB
    LOAD TA         ; LOAD double TOTAL (a b 0 n n)
    ZERO            ; B = 0
    LDXA            ; A = n
    DADD            ; B,A = D,C + B,A
    BCY DONE
    STOR TA         ; A
    STOR TB         ; B
    BR LOOP
DONE:
    HALT            ; carry detected

MAX:    DW 01750
    ; total is a double word value
TOTAL:
TB: DW 00000
TA: DW 00000
