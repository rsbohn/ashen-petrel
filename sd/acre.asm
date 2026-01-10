    ORG 40
START:
    LDXI 0
    LDI 377
    INCA
    DUP
    STOR DB+100,X
    BR P-3
