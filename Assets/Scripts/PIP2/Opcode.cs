/*
 * (C) 2023 Radrat Softworks
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Nofun.PIP2
{
    public enum Opcode
    {
		BREAKPOINT   = 0x0,
		NOP          = 0x1,
		ADD          = 0x2,
		AND          = 0x3,
		MUL          = 0x4,
		DIV          = 0x5,
		DIVU         = 0x6,
		OR           = 0x7,
		XOR          = 0x8,
		SUB          = 0x9,
		SLL          = 0xA,
		SRA          = 0xB,
		SRL          = 0xC,
		NOT          = 0xD,
		NEG 	     = 0xE,
		EXSB	     = 0xF,
		EXSH	     = 0x10,
		MOV 	     = 0x11,
		ADDB	     = 0x12,
		SUBB	     = 0x13,
		ANDB	     = 0x14,
		ORB 	     = 0x15,
		MOVB	     = 0x16,
		ADDH	     = 0x17,
		SUBH	     = 0x18,
		ANDH	     = 0x19,
		ORH 	     = 0x1A,
		MOVH	     = 0x1B,
		SLLi	     = 0x1C,
		SRAi	     = 0x1D,
		SRLi	     = 0x1E,
		ADDQ	     = 0x1F,
		MULQ	     = 0x20,
		ADDBi	     = 0x21,
		ANDBi	     = 0x22,
		ORBi	     = 0x23,
		SLLB	     = 0x24,
		SRLB	     = 0x25,
		SRAB	     = 0x26,
		ADDHi	     = 0x27,
		ANDHi	     = 0x28,
		SLLH	     = 0x29,
        SRLH	     = 0x2A,
        SRAH	     = 0x2B,
		BEQI	     = 0x2C,
		BNEI	     = 0x2D,
		BGEI	     = 0x2E,
		BGEUI	     = 0x2F,
		BGTI	     = 0x30,
		BGTUI	     = 0x31,
		BLEI	     = 0x32,
		BLEUI	     = 0x33,
		BLTI	     = 0x34,
		BLTUI	     = 0x35,
		BEQIB	     = 0x36,
		BNEIB	     = 0x37,
		BGEIB	     = 0x38,
		BGEUIB	     = 0x39,
		BGTIB	     = 0x3A,
		BGTUIB	     = 0x3B,
		BLEIB	     = 0x3C,
		BLEUIB	     = 0x3D,
		BLTIB	     = 0x3E,
		BLTUIB	     = 0x3F,
		LDQ 	     = 0x40,
		JPr 	     = 0x41,
		CALLr	     = 0x42,
		STORE	     = 0x43,
		RESTORE	     = 0x44,
		RET 	     = 0x45,
		KILLTASK     = 0x46,
		SLEEP	     = 0x47,
		SYSCPY	     = 0x48,
		SYSSET	     = 0x49,
		ADDi	     = 0x4A,
		ANDi	     = 0x4B,
		MULi	     = 0x4C,
		DIVi	     = 0x4D,
		DIVUi	     = 0x4E,
		ORi 	     = 0x4F,
		XORi	     = 0x50,
		SUBi	     = 0x51,
		STBd	     = 0x52,
		STHd	     = 0x53,
		STWd	     = 0x54,
		LDBd	     = 0x55,
		LDHd	     = 0x56,
		LDWd	     = 0x57,
		LDBUd	     = 0x58,
		LDHUd	     = 0x59,
		LDI 	     = 0x5A,
		JPl 	     = 0x5B,
		CALLl	     = 0x5C,
		BEQ 	     = 0x5D,
		BNE 	     = 0x5E,
		BGE 	     = 0x5F,
		BGEU	     = 0x60,
		BGT 	     = 0x61,
		BGTU	     = 0x62,
		BLE 	     = 0x63,
		BLEU	     = 0x64,
		BLT 	     = 0x65,
		BLTU	     = 0x66,
		SYSCALL4     = 0x67,
		SYSCALL0     = 0x68,
		SYSCALL1     = 0x69,
		SYSCALL2     = 0x6A,
		SYSCALL3     = 0x6B,
		STBi	     = 0x6C,
		STHi	     = 0x6D,
		STWi	     = 0x6E,
		LDBi	     = 0x6F,
		LDHi	     = 0x70,
		LDWi	     = 0x71,
		LDBUi	     = 0x72,
		LDHUi	     = 0x73
    }
}