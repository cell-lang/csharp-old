SRC-FILES=$(shell ls src/cell/*.cell src/cell/code-gen/*.cell)
RUNTIME-FILES=$(shell ls src/csharp/*)

codegen-dbg: $(SRC-FILES)
	rm -rf tmp/
	mkdir tmp
	cellc project.txt
	mv generated.* tmp/
	g++ -O1 tmp/generated.cpp -o codegen-dbg

test-dbg: codegen-dbg $(RUNTIME-FILES)
	./codegen-dbg test-input.txt
	mcs generated.cs $(RUNTIME-FILES) -out:test-dbg

clean:
	@rm -rf tmp/ codegen-dbg test-dbg
