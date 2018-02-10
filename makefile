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
	cellc-cs projects/compiler.txt
	rm -rf tmp/
	mkdir tmp
	cat generated.cs | bin/apply-hacks > tmp/cellc-cs.cs
	mv generated.cs tmp/
	mcs -nowarn:219 tmp/cellc-cs.cs src/hacks.cs -out:cellc-cs.exe

################################################################################
################################################################################

compiler-test-loop: cellc-cs.exe
	./cellc-cs.exe projects/compiler.txt
	cat generated.cs | bin/apply-hacks > tmp/cellc-cs-1.cs
	mv generated.cs tmp/generated-1.cs
	mcs -nowarn:219 tmp/cellc-cs-1.cs src/hacks.cs -out:cellc-cs-1.exe
	./cellc-cs-1.exe projects/compiler.txt
	cmp generated.cs tmp/generated-1.cs

compiler-test-loop-no-runtime: cellc-cs.exe
	./cellc-cs.exe -nrt projects/compiler.txt
	cat generated.cs | bin/apply-hacks > tmp/cellc-cs-1.cs
	mv generated.cs tmp/generated-1.cs
	mcs -nowarn:219 tmp/cellc-cs-1.cs src/hacks.cs $(RUNTIME-FILES) -out:cellc-cs-1.exe
	./cellc-cs-1.exe -nrt projects/compiler.txt
	cmp generated.cs tmp/generated-1.cs

################################################################################
################################################################################

test.cs: test.cell cellc-cs.exe
	./cellc-cs.exe -d -nrt projects/test.txt
	mv generated.cs test.cs

test.exe: test.cs $(RUNTIME-FILES)
	mcs -nowarn:219 test.cs $(RUNTIME-FILES) -out:test.exe

regression.cs: codegen.exe
	./codegen.exe -d tests/regression.txt
	mv generated.cs regression.cs

regression.exe: regression.cs $(RUNTIME-FILES)
	mcs -nowarn:219 regression.cs $(RUNTIME-FILES) -out:regression.exe

################################################################################
################################################################################

regression-mixed.cs: codegen.exe
	./codegen.exe tests/regression-mixed.txt
	mv generated.cs regression-mixed.cs
	mv interfaces.txt regression-mixed-interfaces.cs

regression-mixed.exe: regression-mixed.cs ../regression-tests/mixed/*.cs
	mcs -nowarn:219 ../regression-tests/mixed/*.cs regression-mixed.cs $(RUNTIME-FILES) -out:regression-mixed.exe

water-sensor.cs: codegen.exe
	./codegen.exe tests/water-sensor.txt
	mv generated.cs water-sensor.cs

water-sensor.exe: water-sensor.cs
	mcs -nowarn:219 water-sensor.cs $(RUNTIME-FILES) -out:water-sensor.exe

send-msgs.cs: codegen.exe
	./codegen.exe tests/send-msgs.txt
	mv generated.cs send-msgs.cs

send-msgs.exe: send-msgs.cs
	mcs -nowarn:219 send-msgs.cs $(RUNTIME-FILES) -out:send-msgs.exe

chat-server.cs: codegen.exe
	./codegen.exe tests/chat-server.txt
	mv generated.cs chat-server.cs

chat-server.exe: chat-server.cs
	mcs -nowarn:219 chat-server.cs $(RUNTIME-FILES) -out:chat-server.exe

chat-server-mixed.cs: codegen.exe
	./codegen.exe tests/chat-server-mixed.txt
	mv generated.cs chat-server-mixed.cs
	mv interfaces.txt chat-server-interface.cs

chat-server-mixed.exe: chat-server-mixed.cs ../download/examples/chat-server/main.cs $(RUNTIME-FILES)
	mcs -nowarn:219 chat-server-mixed.cs ../download/examples/chat-server/main.cs $(RUNTIME-FILES) -out:chat-server-mixed.exe

water-sensor-mixed.cs: codegen.exe
	./codegen.exe tests/water-sensor-mixed.txt
	mv generated.cs water-sensor-mixed.cs
	mv interfaces.txt water-sensor-mixed-interface.cs

water-sensor-mixed.exe: water-sensor-mixed.cs ../download/examples/water-sensor-mixed/main.cs $(RUNTIME-FILES)
	mcs -nowarn:219 water-sensor-mixed.cs ../download/examples/water-sensor-mixed/main.cs $(RUNTIME-FILES) -out:water-sensor-mixed.exe

tests/desugar.txt: $(SRC-FILES)
	cellc -p projects/desugar.txt
	mv dump-opt-code.txt tests/desugar.txt
	rm dump-* generated.cpp

desugar.cs: codegen.exe tests/desugar.txt $(SRC-FILES)
	./codegen.exe tests/desugar.txt
	bin/apply-hacks < generated.cs > desugar.cs
	mv generated.cs tmp/

desugar.exe: desugar.cs $(RUNTIME-FILES)
	mcs -nowarn:219 desugar.cs $(RUNTIME-FILES) -out:desugar.exe

test-mixed.exe: main.cs test.cs $(RUNTIME-FILES)
	mcs -nowarn:219 main.cs test.cs $(RUNTIME-FILES) -out:test-mixed.exe

test.cpp: test.cell
	cellc projects/test.txt
	mv generated.cpp test.cpp
	rm -f generated.h

test: test.cpp
	g++ -ggdb test.cpp -o test

# test.txt: test.cell
# 	cellc -p test-project.txt
# 	mv dump-opt-code.txt test.txt
# 	rm generated.cpp dump-*.txt

# test.exe: test.txt $(RUNTIME-FILES)
# 	./codegen test.txt
# 	mv generated.cs test.cs
# 	mcs test.cs $(RUNTIME-FILES) -out:test.exe

unit-tests.exe: $(RUNTIME-FILES) $(UNIT-TESTS-FILES)
	mcs $(UNIT-TESTS-FILES) $(RUNTIME-FILES) -out:unit-tests.exe

check:
	./gen-html-dbg ../docs/commands.txt        html/commands.html
	./gen-html-dbg ../docs/data.txt            html/data.html
	./gen-html-dbg ../docs/functions.txt       html/functions.html
	./gen-html-dbg ../docs/getting-started.txt html/getting-started.html
	./gen-html-dbg ../docs/imperative.txt      html/imperative.html
	./gen-html-dbg ../docs/index.txt           html/index.html
	./gen-html-dbg ../docs/interface.txt       html/interface.html
	./gen-html-dbg ../docs/miscellanea.txt     html/miscellanea.html
	./gen-html-dbg ../docs/overview.txt        html/overview.html
	./gen-html-dbg ../docs/procedures.txt      html/procedures.html
	./gen-html-dbg ../docs/reactive.txt        html/reactive.html
	./gen-html-dbg ../docs/state.txt           html/state.html
	./gen-html-dbg ../docs/static.txt          html/static.html
	./gen-html-dbg ../docs/typechecking.txt    html/typechecking.html
	./gen-html-dbg ../docs/types.txt           html/types.html
	./gen-html-dbg ../docs/updates.txt         html/updates.html

	../build-website/gen-html ../docs/commands.txt        html-ref/commands.html
	../build-website/gen-html ../docs/data.txt            html-ref/data.html
	../build-website/gen-html ../docs/functions.txt       html-ref/functions.html
	../build-website/gen-html ../docs/getting-started.txt html-ref/getting-started.html
	../build-website/gen-html ../docs/imperative.txt      html-ref/imperative.html
	../build-website/gen-html ../docs/index.txt           html-ref/index.html
	../build-website/gen-html ../docs/interface.txt       html-ref/interface.html
	../build-website/gen-html ../docs/miscellanea.txt     html-ref/miscellanea.html
	../build-website/gen-html ../docs/overview.txt        html-ref/overview.html
	../build-website/gen-html ../docs/procedures.txt      html-ref/procedures.html
	../build-website/gen-html ../docs/reactive.txt        html-ref/reactive.html
	../build-website/gen-html ../docs/state.txt           html-ref/state.html
	../build-website/gen-html ../docs/static.txt          html-ref/static.html
	../build-website/gen-html ../docs/typechecking.txt    html-ref/typechecking.html
	../build-website/gen-html ../docs/types.txt           html-ref/types.html
	../build-website/gen-html ../docs/updates.txt         html-ref/updates.html

	cmp html/commands.html        html-ref/commands.html           
	cmp html/data.html            html-ref/data.html       
	cmp html/functions.html       html-ref/functions.html            
	cmp html/getting-started.html html-ref/getting-started.html          
	cmp html/imperative.html      html-ref/imperative.html             
	cmp html/index.html           html-ref/index.html        
	cmp html/interface.html       html-ref/interface.html            
	cmp html/miscellanea.html     html-ref/miscellanea.html              
	cmp html/overview.html        html-ref/overview.html           
	cmp html/procedures.html      html-ref/procedures.html             
	cmp html/reactive.html        html-ref/reactive.html           
	cmp html/state.html           html-ref/state.html        
	cmp html/static.html          html-ref/static.html         
	cmp html/typechecking.html    html-ref/typechecking.html               
	cmp html/types.html           html-ref/types.html        
	cmp html/updates.html         html-ref/updates.html          

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