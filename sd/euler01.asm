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
    BE SUM
    LDI 5
    DIV, DELB
    BE SUM
    DXBZ P+2
    BR LOOP
    HALT

SUM:                ; (n -- ) TOTAL += n
    LOAD TOTAL
    ADD
    STOR TOTAL
    BR LOOP
MAX:    DW 01750
TOTAL:  DW 00000
