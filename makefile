SRC-FILES=$(shell ls src/cell/*.cell src/cell/code-gen/*.cell)
RUNTIME-FILES=$(shell ls src/csharp/*)
UNIT-TESTS-FILES=$(shell ls src/unit-tests/*)

codegen-dbg tmp/generated.cpp: $(SRC-FILES)
	rm -rf tmp/
	mkdir tmp
	cellc -p projects/codegen.txt
	mv dump-opt-code.txt codegen.txt
	rm dump-*.txt
	mv generated.* tmp/
	g++ -O1 tmp/generated.cpp -o codegen-dbg

codegen-rel: tmp/generated.cpp
	g++ -O1 -DNDEBUG tmp/generated.cpp -o codegen-rel

codegen-opt: tmp/generated.cpp
	g++ -O3 -DNDEBUG tmp/generated.cpp -o codegen-opt

# gen-html-dbg: codegen-dbg $(RUNTIME-FILES)
# 	./codegen gen-html.txt
# 	mcs -debug -d:DEBUG generated.cs $(RUNTIME-FILES) -out:gen-html-dbg

# recompile-generated:
# 	mcs -debug -d:DEBUG generated.cs $(RUNTIME-FILES) -out:gen-html-dbg

codegen.txt: $(SRC-FILES)
	rm -rf tmp/
	mkdir tmp
	cellc -p projects/codegen.txt
	mv dump-opt-code.txt codegen.txt
	rm dump-*.txt
	mv generated.* tmp/

codegen.cs: codegen.txt
	./codegen codegen.txt
	bin/apply-hacks < generated.cs > codegen.cs

codegen.exe: codegen.cs $(RUNTIME-FILES)
	mcs -nowarn:162,168,219,414 codegen.cs $(RUNTIME-FILES) -out:codegen.exe

codegen-dbg.cs: codegen.exe codegen.txt
	./codegen.exe -d codegen.txt
	bin/apply-hacks < generated.cs > codegen-dbg.cs

codegen-dbg.exe: codegen-dbg.cs $(RUNTIME-FILES)
	mcs -nowarn:162,168,219,414 codegen-dbg.cs $(RUNTIME-FILES) -out:codegen-dbg.exe

codegen-rel.exe: codegen.cs $(RUNTIME-FILES)
	mcs -optimize -nowarn:162,168,219,414 codegen.cs $(RUNTIME-FILES) -out:codegen-rel.exe

codegen-2.cs: codegen.exe codegen.txt
	./codegen.exe codegen.txt
	mv generated.cs generated-2.cs
	bin/apply-hacks < generated-2.cs > codegen-2.cs

codegen-2.exe: codegen-2.cs $(RUNTIME-FILES)
	mcs -nowarn:162,168,219,414 codegen-2.cs $(RUNTIME-FILES) -out:codegen-2.exe

compiler.cs: codegen.exe $(SRC-FILES)
	./codegen.exe tests/compiler.txt
	bin/apply-hacks < generated.cs > compiler.cs
	mv generated.cs tmp/

cellc-cs.exe: compiler.cs $(RUNTIME-FILES)
	mcs -nowarn:162,168,219,414 compiler.cs $(RUNTIME-FILES) -out:cellc-cs.exe

compiler-dbg.cs: codegen.exe $(SRC-FILES)
	./codegen.exe -d tests/compiler.txt
	bin/apply-hacks < generated.cs > compiler-dbg.cs
	mv generated.cs tmp/

cellcd-cs.exe: compiler-dbg.cs $(RUNTIME-FILES)
	mcs -nowarn:162,168,219,414 compiler-dbg.cs $(RUNTIME-FILES) -out:cellcd-cs.exe

regression.cs: codegen.exe
	./codegen.exe tests/regression.txt
	mv generated.cs regression.cs

regression.exe: regression.cs
	mcs -nowarn:162,168,219,414 regression.cs $(RUNTIME-FILES) -out:regression.exe

regression-mixed.cs: codegen.exe
	./codegen.exe tests/regression-mixed.txt
	mv generated.cs regression-mixed.cs
	mv interfaces.txt regression-mixed-interfaces.cs

regression-mixed.exe: regression-mixed.cs ../regression-tests/mixed/*.cs
	mcs -nowarn:162,168,219,414 ../regression-tests/mixed/*.cs regression-mixed.cs $(RUNTIME-FILES) -out:regression-mixed.exe

water-sensor.cs: codegen.exe
	./codegen.exe tests/water-sensor.txt
	mv generated.cs water-sensor.cs

water-sensor.exe: water-sensor.cs
	mcs -nowarn:162,168,219,414 water-sensor.cs $(RUNTIME-FILES) -out:water-sensor.exe

send-msgs.cs: codegen.exe
	./codegen.exe tests/send-msgs.txt
	mv generated.cs send-msgs.cs

send-msgs.exe: send-msgs.cs
	mcs -nowarn:162,168,219,414 send-msgs.cs $(RUNTIME-FILES) -out:send-msgs.exe

chat-server.cs: codegen.exe
	./codegen.exe tests/chat-server.txt
	mv generated.cs chat-server.cs

chat-server.exe: chat-server.cs
	mcs -nowarn:162,168,219,414 chat-server.cs $(RUNTIME-FILES) -out:chat-server.exe

chat-server-mixed.cs: codegen.exe
	./codegen.exe tests/chat-server-mixed.txt
	mv generated.cs chat-server-mixed.cs
	mv interfaces.txt chat-server-interface.cs

chat-server-mixed.exe: chat-server-mixed.cs ../download/examples/chat-server/main.cs $(RUNTIME-FILES)
	mcs -nowarn:162,168,219,414 chat-server-mixed.cs ../download/examples/chat-server/main.cs $(RUNTIME-FILES) -out:chat-server-mixed.exe

water-sensor-mixed.cs: codegen.exe
	./codegen.exe tests/water-sensor-mixed.txt
	mv generated.cs water-sensor-mixed.cs
	mv interfaces.txt water-sensor-mixed-interface.cs

water-sensor-mixed.exe: water-sensor-mixed.cs ../download/examples/water-sensor-mixed/main.cs $(RUNTIME-FILES)
	mcs -nowarn:162,168,219,414 water-sensor-mixed.cs ../download/examples/water-sensor-mixed/main.cs $(RUNTIME-FILES) -out:water-sensor-mixed.exe

tests/desugar.txt: $(SRC-FILES)
	cellc -p projects/desugar.txt
	mv dump-opt-code.txt tests/desugar.txt
	rm dump-* generated.cpp

desugar.cs: codegen.exe tests/desugar.txt $(SRC-FILES)
	./codegen.exe tests/desugar.txt
	bin/apply-hacks < generated.cs > desugar.cs
	mv generated.cs tmp/

desugar.exe: desugar.cs $(RUNTIME-FILES)
	mcs -nowarn:162,168,219,414 desugar.cs $(RUNTIME-FILES) -out:desugar.exe

test.cs: test.cell codegen.exe
	cellc -p projects/test.txt
	mv dump-opt-code.txt test.txt
	rm dump-*
	./codegen.exe -d test.txt
	mv generated.cs test.cs

test.exe: test.cs $(RUNTIME-FILES)
	mcs -nowarn:162,168,219,414 test.cs $(RUNTIME-FILES) -out:test.exe

test-mixed.exe: main.cs test.cs $(RUNTIME-FILES)
	mcs -nowarn:162,168,219,414 main.cs test.cs $(RUNTIME-FILES) -out:test-mixed.exe

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
	@make -s soft-clean

soft-clean:
	@rm -f codegen-dbg codegen-rel
	@rm -f codegen.exe codegen.txt
	@rm -rf tmp/
	@rm -f generated.cs generated-2.cs codegen.cs codegen-2.cs
	@rm -f gen-html-dbg gen-html-dbg.mdb
	@rm -f cellc-cs.exe compiler.cs
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
	@rm -f codegen-2.exe codegen-dbg.cs codegen-dbg.exe
	@rm -f unit-tests.exe
	@rm -f debug/*
	@touch debug/stack-trace.txt