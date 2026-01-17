    ORG 200
START:
    LDI 0010
    LDXI 0020
    ADAX, NOP
    LDXA, NOP
    LDI 0030
    SUB, NOP
    TEST, NOP
    BE PASS
FAIL:
    LDI 106
    WIO 00
    LDI 111
    WIO 00
    LDI 114
    WIO 00
    LDI 114
    WIO 00
    LDI 012
    WIO 00
    HALT
PASS:
    LDI 120
    WIO 00
    LDI 101
    WIO 00
    LDI 123
    WIO 00
    LDI 123
    WIO 00
    LDI 012
    WIO 00
    HALT
