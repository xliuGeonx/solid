thisdir = class/Mono.Cecil
include ../../build/rules.make

LIBRARY = Mono.Cecil.dll
LIBRARY_SNK = ../mono.snk
LIBRARY_PACKAGE = none

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll -keyfile:$(LIBRARY_SNK)

NO_TEST = yes

ifneq (net_1_1, $(PROFILE))
NO_INSTALL = yes
endif

include ../../build/library.make
