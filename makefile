SRC-FILES=$(shell ls src/cell/*.cell src/cell/code-gen/*.cell)
RUNTIME-FILES=$(shell ls src/csharp/*)

codegen-dbg: $(SRC-FILES)
	rm -rf tmp/
	mkdir tmp
	cellc project.txt
	mv generated.* tmp/
	g++ -O1 tmp/generated.cpp -o codegen-dbg

gen-html-dbg: codegen-dbg $(RUNTIME-FILES)
	./codegen-dbg gen-html.txt
	mcs -debug -d:DEBUG generated.cs $(RUNTIME-FILES) -out:gen-html-dbg

recompile-generated:
	mcs -debug -d:DEBUG generated.cs $(RUNTIME-FILES) -out:gen-html-dbg

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
	@rm -rf tmp/ codegen-dbg gen-html-dbg generated.cs gen-html-dbg.mdb
