RUNTIME-FILES=$(shell ls src/csharp/)

codegen-dbg:
	rm -rf tmp/
	mkdir tmp
	cellc project.txt
	mv generated.* tmp/
	g++ -O1 tmp/generated.cpp -o codegen-dbg

clean:
	@rm -rf tmp/ codegen-dbg
