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
NEXT:               ; ensure DXBZ runs every iteration
    DXBZ P+2
    BR LOOP
    HALT

SUM:                ; (n -- n)
    DUP             ;TOTAL += n
    ZERO            ; B = 0
    XCH
    LOAD TA
    LOAD TB         ; LOAD double TOTAL (a b 0 n n)
    DADD            ; B,A = D,C + B,A
    BCY DONE
    STOR TB         ; A
    STOR TA         ; B
    BR NEXT
DONE:
    DEL, DEL        ; drop overflow result
    HALT            ; carry detected

MAX:    DW 01747     ; iterate n from 1..999 (Euler 01 sums below 1000)
    ; total is a double word value
TOTAL:
TB: DW 00000
TA: DW 00000
