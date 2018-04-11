SRC-FILES=$(shell ls src/cell/*.cell src/cell/code-gen/*.cell)
CORE-RUNTIME-FILES=$(shell ls src/core/*) src/procs.cs
AUTO-RUNTIME-FILES=$(shell ls src/automata/*)
WRAP-RUNTIME-FILES=$(shell ls src/wrapping/*)
RUNTIME-FILES=$(CORE-RUNTIME-FILES) $(AUTO-RUNTIME-FILES) $(WRAP-RUNTIME-FILES)
UNIT-TESTS-FILES=$(shell ls src/unit-tests/*)

################################################################################
####################### Level 3 AST -> C# code generator #######################

tmp/codegen.cs: $(SRC-FILES)
	cellc-cs -d -nrt projects/codegen.txt
	rm -rf tmp/
	mkdir tmp
	bin/apply-hacks < generated.cs > tmp/codegen.cs
	mv generated.cs tmp/

codegen.exe: tmp/codegen.cs $(CORE-RUNTIME-FILES)
	mcs -nowarn:219 tmp/codegen.cs $(CORE-RUNTIME-FILES) src/hacks.cs -out:codegen.exe

################################################################################
############################# Cell -> C# compiler ##############################

runtime/runtime-sources.cell runtime/runtime-sources-empty.cell: $(RUNTIME-FILES)
	bin/build-runtime-src-file.py src/ runtime/runtime-sources.cell runtime/runtime-sources-empty.cell

cellc-cs: $(SRC-FILES) runtime/runtime-sources.cell runtime/runtime-sources-empty.cell
	cellc projects/compiler.txt
	rm -rf tmp/
	mkdir tmp
	mv generated.* tmp/
	cat tmp/generated.cpp | ../build/bin/ren-fns > tmp/cellc-cs.cpp
	echo >> tmp/cellc-cs.cpp
	echo >> tmp/cellc-cs.cpp
	cat ../build/src/hacks.cpp >> tmp/cellc-cs.cpp
	g++ -O3 -DNDEBUG tmp/cellc-cs.cpp -o cellc-cs

cellc-cs.exe:  $(SRC-FILES) runtime/runtime-sources.cell runtime/runtime-sources-empty.cell
	cellc-cs -nrt projects/compiler.txt
	rm -rf tmp/
	mkdir tmp
	cat generated.cs | bin/apply-hacks > tmp/cellc-cs.cs
	mv generated.cs tmp/
	mcs -nowarn:219 tmp/cellc-cs.cs src/hacks.cs $(CORE-RUNTIME-FILES) -out:cellc-cs.exe

update-cellc-cs.exe:
	mcs -nowarn:219 tmp/cellc-cs.cs src/hacks.cs $(CORE-RUNTIME-FILES) -out:cellc-cs.exe

################################################################################
################################################################################

compiler-test-loop: cellc-cs.exe
	mv cellc-cs.exe tmp/cellc-cs-0.exe
	mv tmp/cellc-cs.cs tmp/cellc-cs-0.cs
	mv tmp/generated.cs tmp/generated-0.cs
	tmp/cellc-cs-0.exe projects/compiler.txt
	cat generated.cs | bin/apply-hacks > tmp/cellc-cs-1.cs
	mv generated.cs tmp/generated-1.cs
	mcs -nowarn:219 tmp/cellc-cs-1.cs src/hacks.cs -out:cellc-cs.exe
	./cellc-cs.exe projects/compiler.txt
	cmp generated.cs tmp/generated-1.cs

compiler-test-loop-no-runtime: cellc-cs.exe
	mv cellc-cs.exe tmp/cellc-cs-0.exe
	mv tmp/cellc-cs.cs tmp/cellc-cs-0.cs
	mv tmp/generated.cs tmp/generated-0.cs
	tmp/cellc-cs-0.exe -nrt projects/compiler.txt
	cat generated.cs | bin/apply-hacks > tmp/cellc-cs-1.cs
	mv generated.cs tmp/generated-1.cs
	mcs -nowarn:219 tmp/cellc-cs-1.cs src/hacks.cs $(RUNTIME-FILES) -out:cellc-cs.exe
	./cellc-cs.exe -nrt projects/compiler.txt
	cmp generated.cs tmp/generated-1.cs

################################################################################
################################################################################

test.cs: test.cell cellc-cs.exe
	./cellc-cs.exe -d -nrt projects/test.txt
	mv generated.cs test.cs

test.exe: test.cs $(RUNTIME-FILES)
	mcs -nowarn:219 test.cs $(RUNTIME-FILES) -out:test.exe

################################################################################
################################################################################

unit-tests.exe: $(RUNTIME-FILES) $(UNIT-TESTS-FILES)
	mcs $(UNIT-TESTS-FILES) $(RUNTIME-FILES) -out:unit-tests.exe

################################################################################
################################################################################

clean:
	@rm -f codegen
	@rm -f codegen-dbg codegen-rel
	@rm -f codegen.exe codegen.txt
	@rm -rf tmp/
	@rm -f generated.cs codegen.cs
	@rm -f gen-html-dbg gen-html-dbg.mdb
	@rm -f cellc-cs.exe compiler.cs
	@rm -f cellc-cs-1.exe
	@rm -f regression.cs
	@rm -f chat-server.cs chat-server.exe
	@rm -f dump-*.txt
	@rm -f test.txt test.cs test.exe test.cpp test
	@rm -f regression.cs regression.exe
	@rm -f generated.cpp generated.h
	@rm -f chat-server-mixed.cs chat-server-interface.cs
	@rm -f water-sensor-mixed*
	@rm -f regression-mixed*
	@rm -f interfaces.txt interfaces.cs
	@rm -f test-mixed*
	@rm -f compiler-dbg.cs cellcd-cs.exe
	@rm -f codegen-dbg.cs codegen-dbg.exe
	@rm -f unit-tests.exe
	@rm -f debug/*
	@touch debug/stack-trace.txt