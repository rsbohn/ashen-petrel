#!/usr/bin/env python3
import sys
from dataclasses import dataclass, field
from pathlib import Path
from typing import Dict, List, Optional, Tuple

HALT_WORD = 0x30F0
BRANCH_MASK = 0xF000
BRANCH_BASE = 0xC000
BRANCH_BACK = 0x0100
BRANCH_INDIRECT = 0x0400
BRANCH_INDEXED = 0x0800
BRANCH_OFFSET_MASK = 0x00FF
LOAD_MASK = 0xF000
LOAD_BASE = 0x4000
LOAD_DB_FLAG = 0x0200
LOAD_X_FLAG = 0x0800
LOAD_I_FLAG = 0x0400
LOAD_DISP_MASK = 0x01FF
LOAD_DISP_SIGN = 0x0100
LOAD_DISP_VALUE_MASK = 0x00FF
IABZ_MASK = 0x7FC0
IABZ_BASE = 0x11C0
IABZ_INDIRECT_FLAG = 0x0080
IABZ_BACK_FLAG = 0x0020
IABZ_DISP_MASK = 0x001F
IXBZ_MASK = 0xF7C0
IXBZ_BASE = 0x1280
IXBZ_INDIRECT_FLAG = 0x0800
IXBZ_BACK_FLAG = 0x0020
IXBZ_DISP_MASK = 0x001F
DXBZ_MASK = 0xF7C0
DXBZ_BASE = 0x12C0
DXBZ_INDIRECT_FLAG = 0x0800
DXBZ_BACK_FLAG = 0x0020
DXBZ_DISP_MASK = 0x001F
BOV_MASK = 0xFFC0
BOV_BASE = 0x1600
BOV_DISP_SIGN = 0x0020
BOV_DISP_MASK = 0x001F
BNOV_MASK = 0xFFC0
BNOV_BASE = 0x1680
BNOV_DISP_SIGN = 0x0020
BNOV_DISP_MASK = 0x001F
BCY_MASK = 0xFFC0
BCY_BASE = 0x1300
BCY_DISP_SIGN = 0x0020
BCY_DISP_MASK = 0x001F
BNCY_MASK = 0xFFC0
BNCY_BASE = 0x1340
BNCY_DISP_SIGN = 0x0020
BNCY_DISP_MASK = 0x001F
BRO_MASK = 0xFFC0
BRO_BASE = 0x1780
BRO_DISP_SIGN = 0x0020
BRO_DISP_MASK = 0x001F
COND_BRANCH_MASK = 0xFE00
COND_BRANCH_BASE = 0xC200
COND_BRANCH_CCF_MASK = 0x01C0
COND_BRANCH_DISP_SIGN = 0x0020
COND_BRANCH_DISP_MASK = 0x001F
STOR_MASK = 0xF200
STOR_BASE = 0x5200
STOR_X_FLAG = 0x0800
STOR_I_FLAG = 0x0400
STOR_DISP_MASK = 0x01FF
INCM_MASK = 0xF200
INCM_BASE = 0xA000
DECM_BASE = 0xA200
LDD_MASK = 0xF200
LDD_BASE = 0xD200
STD_MASK = 0xF200
STD_BASE = 0xE200
IMMEDIATE_MASK = 0xFF00
IMMEDIATE_LDI_BASE = 0x2200
IMMEDIATE_LDXI_BASE = 0x2300
IMMEDIATE_VALUE_MASK = 0x00FF
SCAL_BASE = 0x3100
SCAL_MASK = 0xFF00
SCAL_OPERAND_MASK = 0x00FF
SXIT_BASE = 0x3400
SXIT_MASK = 0xFF00
SXIT_OPERAND_MASK = 0x00FF
SHIFT_MASK = 0xFFC0
ASL_BASE = 0x1000
ASR_BASE = 0x1040
LSL_BASE = 0x1080
LSR_BASE = 0x10C0
SHIFT_COUNT_MASK = 0x003F
DASL_MASK = 0xFDC0
DASL_BASE = 0x1400
DASR_BASE = 0x1440
DLSL_BASE = 0x1480
DLSR_BASE = 0x14C0
DASL_X_FLAG = 0x0200
DDIV_WORD = 0x2179

FORMAT2_MNEMONICS = [
    "NOP",  "DELB", "DDEL", "ZROX", "INCX", "DECX", "ZERO", "DZRO",
    "DCMP", "DADD", "DSUB", "MPYL", "DIVL", "DNEG", "DXCH", "CMP",
    "ADD",  "SUB",  "MPY",  "DIV",  "NEG",  "TEST", "STBX", "DTST",
    "DFLT", "BTST", "XCH",  "INCA", "DECA", "XAX",  "ADAX", "ADXA",
    "DEL",  "ZROB", "LDXB", "STAX", "LDXA", "DUP",  "DDUP", "FLT",
    "FCMP", "FADD", "FSUB", "FMPY", "FDIV", "FNEG", "CAB",  "LCMP",
    "LADD", "LSUB", "LMPY", "LDIV", "NOT",  "OR",   "XOR",  "AND",
    "FIXR", "FIXT", "UNK",  "INCB", "DECB", "XBX",  "ADBX", "ADXB",
]

OPCODES: Dict[str, int] = {}
for opcode, mnemonic in enumerate(FORMAT2_MNEMONICS):
    if mnemonic and mnemonic.strip():
        OPCODES[mnemonic.upper()] = opcode
OPCODES["HALT"] = HALT_WORD
OPCODES["DDIV"] = DDIV_WORD


@dataclass
class AsmLine:
    line_number: int
    address: int
    mnemonic: str
    operand: Optional[str]
    raw_line: str
    values: List[str] = field(default_factory=list)
    words: List[int] = field(default_factory=list)


@dataclass
class SourceLine:
    line_number: int
    raw: str
    asm_line: Optional[AsmLine] = None
    org_address: Optional[int] = None


class AssemblyError(Exception):
    pass


def strip_comment(line: str) -> str:
    index = line.find(";")
    return line[:index] if index >= 0 else line


def extract_label(line: str) -> Tuple[str, str]:
    colon_index = line.find(":")
    if colon_index < 0:
        return "", line
    label = line[:colon_index].strip()
    line = line[colon_index + 1:].strip()
    return label, line


def split_operands(operand: str) -> List[str]:
    tokens: List[str] = []
    parts = [part for part in operand.split(",") if part.strip()]
    for part in parts:
        trimmed = part.strip()
        if not trimmed:
            continue
        inner = [item for item in trimmed.split() if item.strip()]
        tokens.extend(inner)
    return tokens


def to_octal(value: int) -> str:
    return format(value & 0xFFFF, "o").rjust(6, "0")


def try_parse_number(token: str) -> Optional[int]:
    if token.startswith("#"):
        try:
            return int(token[1:], 10)
        except ValueError:
            return None
    if token.startswith("$"):
        try:
            return int(token[1:], 16)
        except ValueError:
            return None
    try:
        return int(token, 8)
    except ValueError:
        return None


def try_parse_number32(token: str) -> Optional[int]:
    if token.startswith("#"):
        try:
            value = int(token[1:], 10)
        except ValueError:
            return None
    elif token.startswith("$"):
        try:
            value = int(token[1:], 16)
        except ValueError:
            return None
    else:
        try:
            value = int(token, 8)
        except ValueError:
            return None
    if value < 0 or value > 0xFFFFFFFF:
        return None
    return value


def try_parse_number64(token: str) -> Optional[int]:
    if token.startswith("#"):
        try:
            value = int(token[1:], 10)
        except ValueError:
            return None
    elif token.startswith("$"):
        try:
            value = int(token[1:], 16)
        except ValueError:
            return None
    else:
        try:
            value = int(token, 8)
        except ValueError:
            return None
    if value < 0 or value > 0xFFFFFFFFFFFFFFFF:
        return None
    return value


def try_parse_text_literal(operand: str) -> Tuple[bool, str, str]:
    trimmed = operand.strip()
    if not trimmed.startswith("/"):
        return False, "", "TXT requires /text/"
    last_slash = trimmed.rfind("/")
    if last_slash == 0:
        return False, "", "TXT requires closing /"
    if last_slash < len(trimmed) - 1 and trimmed[last_slash + 1:].strip():
        return False, "", "TXT has trailing characters after closing /"
    return True, trimmed[1:last_slash], ""


def try_resolve_qualified_symbol(token: str, symbols: Dict[str, int]) -> Optional[int]:
    key = token.lower()
    if key in symbols:
        return symbols[key]

    dot_index = token.rfind(".")
    if dot_index <= 0 or dot_index == len(token) - 1:
        return None
    base_name = token[:dot_index]
    qualifier = token[dot_index + 1:]
    base_key = base_name.lower()
    if base_key not in symbols:
        return None
    base_value = symbols[base_key]
    if qualifier.lower() == "high":
        return base_value
    if qualifier.lower() == "low":
        return (base_value + 1) & 0x7FFF
    return None


def try_resolve_symbol_or_dot(token: str, address: int, symbols: Dict[str, int]) -> Optional[int]:
    if token == ".":
        return address
    return try_resolve_qualified_symbol(token, symbols)


def try_resolve_value(token: str, symbols: Dict[str, int], address: int) -> Optional[int]:
    value = try_parse_number(token)
    if value is not None:
        return value
    if token == ".":
        return address
    return try_resolve_qualified_symbol(token, symbols)


def try_resolve_value32(token: str, symbols: Dict[str, int], address: int) -> Optional[int]:
    value = try_parse_number32(token)
    if value is not None:
        return value
    if token == ".":
        return address
    return try_resolve_qualified_symbol(token, symbols)


def try_resolve_value64(token: str, symbols: Dict[str, int], address: int) -> Optional[int]:
    value = try_parse_number64(token)
    if value is not None:
        return value
    if token == ".":
        return address
    return try_resolve_qualified_symbol(token, symbols)


def try_resolve_pc_relative_token(token: str) -> Optional[str]:
    if len(token) < 3:
        return None
    if token[0] not in (".", "P", "p"):
        return None
    sign = token[1]
    if sign not in ("+", "-"):
        return None
    magnitude_text = token[2:]
    magnitude = try_parse_number(magnitude_text)
    if magnitude is None:
        return None
    return f".{sign}{format(magnitude, 'o')}"


def try_resolve_relative_base(base_part: str, address: int, symbols: Dict[str, int]) -> Tuple[bool, str, str]:
    if try_parse_number(base_part) is not None:
        return True, base_part, ""
    resolved = try_resolve_pc_relative_token(base_part)
    if resolved is not None:
        return True, resolved, ""
    target = try_resolve_symbol_or_dot(base_part, address, symbols)
    if target is not None:
        displacement = target - address
        direction = "-" if displacement < 0 else "+"
        magnitude = abs(displacement)
        return True, f".{direction}{format(magnitude, 'o')}", ""
    return False, "", f"unknown label '{base_part}'"


def try_resolve_base(base_part: str, address: int, symbols: Dict[str, int]) -> Tuple[bool, str, str]:
    if try_parse_number(base_part) is not None:
        return True, base_part, ""
    resolved_value = try_resolve_symbol_or_dot(base_part, address, symbols)
    if resolved_value is not None:
        return True, format(resolved_value, "o"), ""
    resolved = try_resolve_pc_relative_token(base_part)
    if resolved is not None:
        return True, resolved, ""

    plus_index = base_part.find("+")
    if 0 < plus_index < len(base_part) - 1:
        prefix = base_part[:plus_index + 1]
        label = base_part[plus_index + 1:].strip()
        number = try_parse_number(label)
        if number is not None:
            return True, prefix + format(number, "o"), ""
        value = try_resolve_symbol_or_dot(label, address, symbols)
        if value is not None:
            return True, prefix + format(value, "o"), ""

    minus_index = base_part.find("-")
    if 0 < minus_index < len(base_part) - 1:
        prefix = base_part[:minus_index + 1]
        label = base_part[minus_index + 1:].strip()
        number = try_parse_number(label)
        if number is not None:
            return True, prefix + format(number, "o"), ""
        value = try_resolve_symbol_or_dot(label, address, symbols)
        if value is not None:
            return True, prefix + format(value, "o"), ""

    return False, "", f"unknown label '{base_part}'"


def is_operand_mnemonic(mnemonic: str) -> bool:
    upper = mnemonic.upper()
    return upper in {
        "BR", "BRO", "BN", "BL", "BE", "BLE", "BG", "BNE", "BGE", "BA",
        "BOV", "BNOV", "BCY", "BNCY", "WIO", "RIO", "LDI", "LDXI", "LOAD",
        "STOR", "LDD", "STD", "IABZ", "IXBZ", "DXBZ", "SCAL", "SXIT",
        "ASL", "ASR", "LSL", "LSR", "DASL", "DASR", "DLSL", "DLSR", "INCM",
        "DECM",
    }


def is_relative_mnemonic(mnemonic: str) -> bool:
    upper = mnemonic.upper()
    return upper in {
        "BR", "BRO", "BN", "BL", "BE", "BLE", "BG", "BNE", "BGE", "BA",
        "BOV", "BNOV", "BCY", "BNCY",
    }


def try_resolve_operand(
    mnemonic: str,
    operand: str,
    address: int,
    symbols: Dict[str, int],
) -> Tuple[bool, str, str]:
    parts = [part for part in operand.split(",") if part.strip()]
    if not parts:
        return False, "", f"invalid operand '{operand}'"
    base_part = parts[0].strip()
    suffix = ""
    if len(parts) > 1:
        suffix = "," + ",".join(parts[1:]).strip()

    upper = mnemonic.upper()
    if upper == "LOAD":
        if base_part.upper().startswith("DB+"):
            ok, resolved_base, error = try_resolve_base(base_part, address, symbols)
            if not ok:
                return False, "", error
            return True, resolved_base + suffix, ""
        ok, resolved_base, error = try_resolve_relative_base(base_part, address, symbols)
        if not ok:
            return False, "", error
        return True, resolved_base + suffix, ""

    if is_relative_mnemonic(upper):
        ok, resolved_base, error = try_resolve_relative_base(base_part, address, symbols)
        if not ok:
            return False, "", error
        return True, resolved_base + suffix, ""

    ok, resolved_base, error = try_resolve_base(base_part, address, symbols)
    if not ok:
        return False, "", error
    return True, resolved_base + suffix, ""


def try_parse_octal(token: str) -> Optional[int]:
    if token.strip() == "":
        return None
    try:
        value = int(token, 8)
    except ValueError:
        return None
    if value < 0:
        return None
    return value


def try_parse_pc_relative(
    base_part: str,
    require_prefix: bool,
) -> Optional[Tuple[str, str]]:
    if not base_part:
        return None
    if base_part[0] == ".":
        if len(base_part) < 3:
            return None
        direction = base_part[1]
        if direction not in ("+", "-"):
            return None
        return direction, base_part[2:]
    if (base_part[0] in ("P", "p")
            and len(base_part) >= 3
            and base_part[1] in ("+", "-")):
        return base_part[1], base_part[2:]
    if require_prefix:
        return None
    return "+", base_part


def try_assemble_branch(operand: str) -> Optional[int]:
    if not operand.strip():
        return None
    parts = [part for part in operand.strip().split(",") if part.strip()]
    if not parts:
        return None
    base_part = parts[0].strip()
    parsed = try_parse_pc_relative(base_part, True)
    if parsed is None:
        return None
    direction, offset_text = parsed
    offset = try_parse_octal(offset_text)
    if offset is None or offset > BRANCH_OFFSET_MASK:
        return None
    opcode = BRANCH_BASE | offset
    if direction == "-":
        opcode |= BRANCH_BACK
    for suffix in parts[1:]:
        suffix = suffix.strip()
        if suffix.upper() == "I":
            opcode |= BRANCH_INDIRECT
        elif suffix.upper() == "X":
            opcode |= BRANCH_INDEXED
        else:
            return None
    return opcode


def try_assemble_load(operand: str) -> Optional[int]:
    if not operand.strip():
        return None
    parts = [part for part in operand.strip().split(",") if part.strip()]
    if not parts:
        return None
    base_part = parts[0].strip()
    if base_part.upper().startswith("DB"):
        if len(base_part) < 4 or base_part[2] != "+":
            return None
        db_offset_text = base_part[3:]
        offset = try_parse_octal(db_offset_text)
        if offset is None or offset > LOAD_DISP_MASK:
            return None
        opcode = LOAD_BASE | LOAD_DB_FLAG | offset
        for suffix in parts[1:]:
            suffix = suffix.strip()
            if suffix.upper() == "I":
                opcode |= LOAD_I_FLAG
            elif suffix.upper() == "X":
                opcode |= LOAD_X_FLAG
            else:
                return None
        return opcode

    parsed = try_parse_pc_relative(base_part, False)
    if parsed is None:
        return None
    direction, offset_text = parsed
    offset = try_parse_octal(offset_text)
    if offset is None or offset > LOAD_DISP_VALUE_MASK:
        return None
    opcode = LOAD_BASE | offset
    if direction == "-":
        opcode |= LOAD_DISP_SIGN
    for suffix in parts[1:]:
        suffix = suffix.strip()
        if suffix.upper() == "I":
            opcode |= LOAD_I_FLAG
        elif suffix.upper() == "X":
            opcode |= LOAD_X_FLAG
        else:
            return None
    return opcode


def try_assemble_stor(operand: str) -> Optional[int]:
    if not operand.strip():
        return None
    parts = [part for part in operand.strip().split(",") if part.strip()]
    if not parts:
        return None
    base_part = parts[0].strip()
    if not base_part:
        return None
    offset_text = base_part
    if base_part.upper().startswith("DB"):
        if len(base_part) < 4 or base_part[2] != "+":
            return None
        offset_text = base_part[3:]
    offset = try_parse_octal(offset_text)
    if offset is None or offset > STOR_DISP_MASK:
        return None
    opcode = STOR_BASE | offset
    for suffix in parts[1:]:
        suffix = suffix.strip()
        if suffix.upper() == "I":
            opcode |= STOR_I_FLAG
        elif suffix.upper() == "X":
            opcode |= STOR_X_FLAG
        else:
            return None
    return opcode


def try_assemble_ldd(operand: str) -> Optional[int]:
    if not operand.strip():
        return None
    parts = [part for part in operand.strip().split(",") if part.strip()]
    if not parts:
        return None
    base_part = parts[0].strip()
    if not base_part:
        return None
    offset_text = base_part
    if base_part.upper().startswith("DB"):
        if len(base_part) < 4 or base_part[2] != "+":
            return None
        offset_text = base_part[3:]
    offset = try_parse_octal(offset_text)
    if offset is None or offset > STOR_DISP_MASK:
        return None
    opcode = LDD_BASE | offset
    for suffix in parts[1:]:
        suffix = suffix.strip()
        if suffix.upper() == "I":
            opcode |= STOR_I_FLAG
        elif suffix.upper() == "X":
            opcode |= STOR_X_FLAG
        else:
            return None
    return opcode


def try_assemble_std(operand: str) -> Optional[int]:
    if not operand.strip():
        return None
    parts = [part for part in operand.strip().split(",") if part.strip()]
    if not parts:
        return None
    base_part = parts[0].strip()
    if not base_part:
        return None
    offset_text = base_part
    if base_part.upper().startswith("DB"):
        if len(base_part) < 4 or base_part[2] != "+":
            return None
        offset_text = base_part[3:]
    offset = try_parse_octal(offset_text)
    if offset is None or offset > STOR_DISP_MASK:
        return None
    opcode = STD_BASE | offset
    for suffix in parts[1:]:
        suffix = suffix.strip()
        if suffix.upper() == "I":
            opcode |= STOR_I_FLAG
        elif suffix.upper() == "X":
            opcode |= STOR_X_FLAG
        else:
            return None
    return opcode


def try_assemble_iabz(operand: str) -> Optional[int]:
    if not operand.strip():
        return None
    parts = [part for part in operand.strip().split(",") if part.strip()]
    if not parts:
        return None
    base_part = parts[0].strip()
    parsed = try_parse_pc_relative(base_part, False)
    if parsed is None:
        return None
    direction, offset_text = parsed
    offset = try_parse_octal(offset_text)
    if offset is None or offset > IABZ_DISP_MASK:
        return None
    opcode = IABZ_BASE | offset
    if direction == "-":
        opcode |= IABZ_BACK_FLAG
    for suffix in parts[1:]:
        suffix = suffix.strip()
        if suffix.upper() == "I":
            opcode |= IABZ_INDIRECT_FLAG
        else:
            return None
    return opcode


def try_assemble_ixbz(operand: str) -> Optional[int]:
    if not operand.strip():
        return None
    parts = [part for part in operand.strip().split(",") if part.strip()]
    if not parts:
        return None
    base_part = parts[0].strip()
    parsed = try_parse_pc_relative(base_part, False)
    if parsed is None:
        return None
    direction, offset_text = parsed
    offset = try_parse_octal(offset_text)
    if offset is None or offset > IXBZ_DISP_MASK:
        return None
    opcode = IXBZ_BASE | offset
    if direction == "-":
        opcode |= IXBZ_BACK_FLAG
    for suffix in parts[1:]:
        suffix = suffix.strip()
        if suffix.upper() == "I":
            opcode |= IXBZ_INDIRECT_FLAG
        else:
            return None
    return opcode


def try_assemble_dxbz(operand: str) -> Optional[int]:
    if not operand.strip():
        return None
    parts = [part for part in operand.strip().split(",") if part.strip()]
    if not parts:
        return None
    base_part = parts[0].strip()
    parsed = try_parse_pc_relative(base_part, False)
    if parsed is None:
        return None
    direction, offset_text = parsed
    offset = try_parse_octal(offset_text)
    if offset is None or offset > DXBZ_DISP_MASK:
        return None
    opcode = DXBZ_BASE | offset
    if direction == "-":
        opcode |= DXBZ_BACK_FLAG
    for suffix in parts[1:]:
        suffix = suffix.strip()
        if suffix.upper() == "I":
            opcode |= DXBZ_INDIRECT_FLAG
        else:
            return None
    return opcode


def try_assemble_short_branch(
    operand: str,
    base_opcode: int,
    disp_mask: int,
    disp_sign: int,
) -> Optional[int]:
    if not operand.strip():
        return None
    base_part = operand.strip()
    parsed = try_parse_pc_relative(base_part, True)
    if parsed is None:
        return None
    direction, offset_text = parsed
    offset = try_parse_octal(offset_text)
    if offset is None or offset > disp_mask:
        return None
    opcode = base_opcode | offset
    if direction == "-":
        opcode |= disp_sign
    return opcode


def try_assemble_cond_branch(operand: str, ccf: int) -> Optional[int]:
    if not operand.strip():
        return None
    base_part = operand.strip()
    parsed = try_parse_pc_relative(base_part, False)
    if parsed is None:
        return None
    direction, offset_text = parsed
    offset = try_parse_octal(offset_text)
    if offset is None or offset > COND_BRANCH_DISP_MASK:
        return None
    opcode = COND_BRANCH_BASE | (ccf << 6) | offset
    if direction == "-":
        opcode |= COND_BRANCH_DISP_SIGN
    return opcode


def try_assemble_immediate(operand: str, base_opcode: int) -> Optional[int]:
    value = try_parse_octal(operand)
    if value is None or value > IMMEDIATE_VALUE_MASK:
        return None
    return base_opcode | value


def try_assemble_scal(operand: str) -> Optional[int]:
    value = try_parse_octal(operand)
    if value is None or value > SCAL_OPERAND_MASK:
        return None
    return SCAL_BASE | value


def try_assemble_sxit(operand: str) -> Optional[int]:
    value = try_parse_octal(operand)
    if value is None or value > SXIT_OPERAND_MASK:
        return None
    return SXIT_BASE | value


def try_assemble_shift(operand: str, base_opcode: int) -> Optional[int]:
    value = try_parse_number(operand)
    if value is None or value < 0 or value > SHIFT_COUNT_MASK:
        return None
    return base_opcode | value


def try_assemble_dasl(operand: str, base_opcode: int) -> Optional[int]:
    if not operand.strip():
        return None
    parts = [part for part in operand.strip().split(",") if part.strip()]
    if not parts:
        return None
    value = try_parse_number(parts[0].strip())
    if value is None or value < 0 or value > SHIFT_COUNT_MASK:
        return None
    opcode = base_opcode | value
    for suffix in parts[1:]:
        if suffix.strip().upper() == "X":
            opcode |= DASL_X_FLAG
        else:
            return None
    return opcode


def try_assemble_mem_adjust(operand: str, base_opcode: int) -> Optional[int]:
    if not operand.strip():
        return None
    parts = [part for part in operand.strip().split(",") if part.strip()]
    if not parts:
        return None
    base_part = parts[0].strip()
    if not base_part:
        return None
    offset_text = base_part
    if base_part.upper().startswith("DB"):
        if len(base_part) < 4 or base_part[2] != "+":
            return None
        offset_text = base_part[3:]
    offset = try_parse_octal(offset_text)
    if offset is None or offset > STOR_DISP_MASK:
        return None
    opcode = base_opcode | offset
    for suffix in parts[1:]:
        suffix = suffix.strip()
        if suffix.upper() == "I":
            opcode |= STOR_I_FLAG
        elif suffix.upper() == "X":
            opcode |= STOR_X_FLAG
        else:
            return None
    return opcode


def try_assemble(mnemonic: str) -> Optional[int]:
    return OPCODES.get(mnemonic.upper())


def try_assemble_with_operand(mnemonic: str, operand: str) -> Optional[int]:
    upper = mnemonic.upper()
    if upper == "BR":
        return try_assemble_branch(operand)
    if upper == "HALT":
        if operand.strip() == "0":
            return HALT_WORD
        return None
    if upper == "SCAL":
        return try_assemble_scal(operand)
    if upper == "SXIT":
        return try_assemble_sxit(operand)
    if upper == "ASL":
        return try_assemble_shift(operand, ASL_BASE)
    if upper == "ASR":
        return try_assemble_shift(operand, ASR_BASE)
    if upper == "LSL":
        return try_assemble_shift(operand, LSL_BASE)
    if upper == "LSR":
        return try_assemble_shift(operand, LSR_BASE)
    if upper == "DASL":
        return try_assemble_dasl(operand, DASL_BASE)
    if upper == "DASR":
        return try_assemble_dasl(operand, DASR_BASE)
    if upper == "DLSL":
        return try_assemble_dasl(operand, DLSL_BASE)
    if upper == "DLSR":
        return try_assemble_dasl(operand, DLSR_BASE)
    if upper == "INCM":
        return try_assemble_mem_adjust(operand, INCM_BASE)
    if upper == "DECM":
        return try_assemble_mem_adjust(operand, DECM_BASE)
    if upper == "LOAD":
        return try_assemble_load(operand)
    if upper == "IABZ":
        return try_assemble_iabz(operand)
    if upper == "IXBZ":
        return try_assemble_ixbz(operand)
    if upper == "BN":
        return try_assemble_cond_branch(operand, 0)
    if upper == "BL":
        return try_assemble_cond_branch(operand, 1)
    if upper == "BE":
        return try_assemble_cond_branch(operand, 2)
    if upper == "BLE":
        return try_assemble_cond_branch(operand, 3)
    if upper == "BG":
        return try_assemble_cond_branch(operand, 4)
    if upper == "BNE":
        return try_assemble_cond_branch(operand, 5)
    if upper == "BGE":
        return try_assemble_cond_branch(operand, 6)
    if upper == "BA":
        return try_assemble_cond_branch(operand, 7)
    if upper == "BOV":
        return try_assemble_short_branch(operand, BOV_BASE, BOV_DISP_MASK, BOV_DISP_SIGN)
    if upper == "BNOV":
        return try_assemble_short_branch(operand, BNOV_BASE, BNOV_DISP_MASK, BNOV_DISP_SIGN)
    if upper == "BCY":
        return try_assemble_short_branch(operand, BCY_BASE, BCY_DISP_MASK, BCY_DISP_SIGN)
    if upper == "BNCY":
        return try_assemble_short_branch(operand, BNCY_BASE, BNCY_DISP_MASK, BNCY_DISP_SIGN)
    if upper == "BRO":
        return try_assemble_short_branch(operand, BRO_BASE, BRO_DISP_MASK, BRO_DISP_SIGN)
    if upper == "DXBZ":
        return try_assemble_dxbz(operand)
    if upper == "STOR":
        return try_assemble_stor(operand)
    if upper == "LDD":
        return try_assemble_ldd(operand)
    if upper == "STD":
        return try_assemble_std(operand)
    if upper == "LDI":
        return try_assemble_immediate(operand, IMMEDIATE_LDI_BASE)
    if upper == "LDXI":
        return try_assemble_immediate(operand, IMMEDIATE_LDXI_BASE)
    if upper == "WIO":
        device = try_parse_octal(operand)
        if device is None or device > 0x0F:
            return None
        return 0x3000 | (0x09 << 4) | device
    if upper == "RIO":
        device = try_parse_octal(operand)
        if device is None or device > 0x0F:
            return None
        return 0x3000 | (0x08 << 4) | device
    return None


def build_srec(bytes_out: Dict[int, int]) -> str:
    if not bytes_out:
        return "S9030000FC\n"
    ordered = sorted(bytes_out.items())
    lines: List[str] = []
    index = 0
    while index < len(ordered):
        start_address = ordered[index][0]
        record_bytes: List[int] = []
        current_address = start_address
        while index < len(ordered) and ordered[index][0] == current_address and len(record_bytes) < 16:
            record_bytes.append(ordered[index][1])
            current_address += 1
            index += 1
        lines.append(build_s1_record(start_address, record_bytes))
    lines.append("S9030000FC")
    return "\n".join(lines) + "\n"


def build_s1_record(address: int, data: List[int]) -> str:
    count = len(data) + 3
    checksum_sum = count + ((address >> 8) & 0xFF) + (address & 0xFF)
    for value in data:
        checksum_sum += value
    checksum = (~checksum_sum) & 0xFF
    data_text = "".join(f"{value:02X}" for value in data)
    return f"S1{count:02X}{address:04X}{data_text}{checksum:02X}"


def format_listing_line(address: Optional[int], words: List[int], source: str) -> str:
    source = source.rstrip("\n")
    if address is None:
        return source.rstrip()
    addr_text = to_octal(address)
    data_text = " ".join(to_octal(word) for word in words)
    if data_text:
        return f"{addr_text} {data_text}  {source.rstrip()}"
    return f"{addr_text}  {source.rstrip()}"


def assemble(path: str) -> Tuple[List[SourceLine], Dict[int, int]]:
    symbols: Dict[str, int] = {}
    asm_lines: List[AsmLine] = []
    source_lines: List[SourceLine] = []
    address = 0
    origin_set = False

    with open(path, "r", encoding="utf-8") as handle:
        for line_number, raw_line in enumerate(handle, 1):
            stripped = strip_comment(raw_line).strip()
            if not stripped:
                source_lines.append(SourceLine(line_number=line_number, raw=raw_line.rstrip("\n")))
                continue

            label, remainder = extract_label(stripped)
            if label:
                key = label.lower()
                if key in symbols:
                    raise AssemblyError(f"asm {path}:{line_number}: duplicate label '{label}'")
                symbols[key] = address & 0x7FFF

            if not remainder:
                source_lines.append(SourceLine(line_number=line_number, raw=raw_line.rstrip("\n")))
                continue

            parts = remainder.split(None, 1)
            if not parts:
                source_lines.append(SourceLine(line_number=line_number, raw=raw_line.rstrip("\n")))
                continue

            mnemonic = parts[0]
            operand = parts[1].strip() if len(parts) > 1 else None
            upper = mnemonic.upper()

            if upper == "ORG":
                if operand is None:
                    raise AssemblyError(f"asm {path}:{line_number}: invalid ORG operand ''")
                org_value = try_parse_number(operand)
                if org_value is None:
                    raise AssemblyError(f"asm {path}:{line_number}: invalid ORG operand '{operand}'")
                address = org_value & 0x7FFF
                if not origin_set:
                    origin_set = True
                source_lines.append(SourceLine(line_number=line_number, raw=raw_line.rstrip("\n"), org_address=address))
                continue

            if upper == "DW":
                if operand is None:
                    raise AssemblyError(f"asm {path}:{line_number}: DW requires at least one value")
                values = split_operands(operand)
                asm_line = AsmLine(line_number, address, "DW", None, raw_line.rstrip("\n"), values=values)
                asm_lines.append(asm_line)
                source_lines.append(SourceLine(line_number=line_number, raw=raw_line.rstrip("\n"), asm_line=asm_line))
                address = (address + len(values)) & 0x7FFF
                continue

            if upper == "TXT":
                if operand is None:
                    raise AssemblyError(f"asm {path}:{line_number}: TXT requires /text/")
                ok, text, error = try_parse_text_literal(operand)
                if not ok:
                    raise AssemblyError(f"asm {path}:{line_number}: {error}")
                asm_line = AsmLine(line_number, address, "TXT", operand, raw_line.rstrip("\n"))
                asm_lines.append(asm_line)
                source_lines.append(SourceLine(line_number=line_number, raw=raw_line.rstrip("\n"), asm_line=asm_line))
                address = (address + len(text)) & 0x7FFF
                continue

            if upper == "DD":
                if operand is None:
                    raise AssemblyError(f"asm {path}:{line_number}: DD requires at least one value")
                values = split_operands(operand)
                asm_line = AsmLine(line_number, address, "DD", None, raw_line.rstrip("\n"), values=values)
                asm_lines.append(asm_line)
                source_lines.append(SourceLine(line_number=line_number, raw=raw_line.rstrip("\n"), asm_line=asm_line))
                address = (address + (len(values) * 2)) & 0x7FFF
                continue

            if upper == "DQ":
                if operand is None:
                    raise AssemblyError(f"asm {path}:{line_number}: DQ requires at least one value")
                values = split_operands(operand)
                asm_line = AsmLine(line_number, address, "DQ", None, raw_line.rstrip("\n"), values=values)
                asm_lines.append(asm_line)
                source_lines.append(SourceLine(line_number=line_number, raw=raw_line.rstrip("\n"), asm_line=asm_line))
                address = (address + (len(values) * 4)) & 0x7FFF
                continue

            asm_line = AsmLine(line_number, address, mnemonic, operand, raw_line.rstrip("\n"))
            asm_lines.append(asm_line)
            source_lines.append(SourceLine(line_number=line_number, raw=raw_line.rstrip("\n"), asm_line=asm_line))
            address = (address + 1) & 0x7FFF

    memory: Dict[int, int] = {}
    for asm_line in asm_lines:
        mnemonic = asm_line.mnemonic
        operand = asm_line.operand
        upper = mnemonic.upper()

        if upper == "DW":
            write_address = asm_line.address
            for token in asm_line.values:
                value = try_resolve_value(token, symbols, write_address)
                if value is None:
                    raise AssemblyError(f"asm {path}:{asm_line.line_number}: invalid literal '{token}'")
                memory[write_address] = value & 0xFFFF
                asm_line.words.append(value & 0xFFFF)
                write_address = (write_address + 1) & 0x7FFF
            continue

        if upper == "DD":
            write_address = asm_line.address
            for token in asm_line.values:
                value = try_resolve_value32(token, symbols, write_address)
                if value is None:
                    raise AssemblyError(f"asm {path}:{asm_line.line_number}: invalid literal '{token}'")
                high = (value >> 16) & 0xFFFF
                low = value & 0xFFFF
                memory[write_address] = high
                asm_line.words.append(high)
                write_address = (write_address + 1) & 0x7FFF
                memory[write_address] = low
                asm_line.words.append(low)
                write_address = (write_address + 1) & 0x7FFF
            continue

        if upper == "TXT":
            if operand is None:
                raise AssemblyError(f"asm {path}:{asm_line.line_number}: TXT requires /text/")
            ok, text, error = try_parse_text_literal(operand)
            if not ok:
                raise AssemblyError(f"asm {path}:{asm_line.line_number}: {error}")
            write_address = asm_line.address
            for ch in text:
                value = ord(ch) & 0xFF
                memory[write_address] = value
                asm_line.words.append(value)
                write_address = (write_address + 1) & 0x7FFF
            continue

        if upper == "DQ":
            write_address = asm_line.address
            for token in asm_line.values:
                value = try_resolve_value64(token, symbols, write_address)
                if value is None:
                    raise AssemblyError(f"asm {path}:{asm_line.line_number}: invalid literal '{token}'")
                words = [
                    (value >> 48) & 0xFFFF,
                    (value >> 32) & 0xFFFF,
                    (value >> 16) & 0xFFFF,
                    value & 0xFFFF,
                ]
                for word in words:
                    memory[write_address] = word
                    asm_line.words.append(word)
                    write_address = (write_address + 1) & 0x7FFF
            continue

        if mnemonic.endswith(","):
            first_mnemonic = mnemonic.rstrip(",")
            if not operand:
                raise AssemblyError(f"asm {path}:{asm_line.line_number}: missing second opcode")
            second_parts = [part for part in operand.split() if part.strip()]
            if len(second_parts) != 1:
                raise AssemblyError(f"asm {path}:{asm_line.line_number}: invalid packed opcodes '{asm_line.raw_line}'")
            first_opcode = try_assemble(first_mnemonic)
            second_opcode = try_assemble(second_parts[0])
            if first_opcode is None or second_opcode is None:
                raise AssemblyError(f"asm {path}:{asm_line.line_number}: unknown mnemonic in '{asm_line.raw_line}'")
            packed = ((first_opcode << 6) | second_opcode) & 0xFFFF
            memory[asm_line.address] = packed
            asm_line.words.append(packed)
            continue

        if operand is not None:
            ok, resolved_operand, error = try_resolve_operand(mnemonic, operand, asm_line.address, symbols)
            if not ok:
                raise AssemblyError(f"asm {path}:{asm_line.line_number}: {error}")
            opcode = try_assemble_with_operand(mnemonic, resolved_operand)
            if opcode is not None:
                memory[asm_line.address] = opcode
                asm_line.words.append(opcode)
                continue
            if is_operand_mnemonic(mnemonic):
                raise AssemblyError(
                    f"asm {path}:{asm_line.line_number}: invalid operand '{operand}' for {mnemonic}"
                )
        else:
            if is_operand_mnemonic(mnemonic):
                raise AssemblyError(f"asm {path}:{asm_line.line_number}: {mnemonic} requires an operand")

        opcode = try_assemble(mnemonic)
        if opcode is not None:
            memory[asm_line.address] = opcode
            asm_line.words.append(opcode)
            continue

        raise AssemblyError(f"asm {path}:{asm_line.line_number}: unknown mnemonic '{mnemonic}'")

    return source_lines, memory


def main() -> int:
    if len(sys.argv) != 2:
        print("usage: python3 scripts/asm_hp3k.py <input_file>", file=sys.stderr)
        return 1

    input_path = sys.argv[1]
    try:
        source_lines, memory = assemble(input_path)
    except AssemblyError as exc:
        print(str(exc), file=sys.stderr)
        return 1

    bytes_out: Dict[int, int] = {}
    for address in sorted(memory.keys()):
        word = memory[address] & 0xFFFF
        byte_address = (address & 0x7FFF) * 2
        bytes_out[byte_address] = (word >> 8) & 0xFF
        bytes_out[byte_address + 1] = word & 0xFF

    output_base = Path(input_path).stem
    output_srec_path = Path(f"{output_base}.srec")
    output_list_path = Path(f"{output_base}.list")

    srec_text = build_srec(bytes_out)
    with output_srec_path.open("w", encoding="ascii") as handle:
        handle.write(srec_text)

    listing_lines: List[str] = []
    for source_line in source_lines:
        if source_line.asm_line is not None:
            listing_lines.append(
                format_listing_line(
                    source_line.asm_line.address,
                    source_line.asm_line.words,
                    source_line.raw,
                )
            )
            continue
        if source_line.org_address is not None:
            listing_lines.append(format_listing_line(source_line.org_address, [], source_line.raw))
            continue
        raw = source_line.raw.rstrip("\n")
        if raw.strip() == "":
            listing_lines.append("")
        else:
            listing_lines.append(raw.rstrip())

    with output_list_path.open("w", encoding="utf-8") as handle:
        handle.write("\n".join(listing_lines) + "\n")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
