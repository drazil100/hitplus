ifeq ($(OS),Windows_NT)
	CSC = csc
	REFERENCES =
else
	CSC = mcs
	REFERENCES = -sdk:4.5 -r:System.Windows.Forms,System.Drawing,System.Core
endif

all: HitPlus.exe

HitPlus.exe: $(wildcard *.cs)
	$(CSC) /out:HitPlus.exe /win32icon:medal.ico $(REFERENCES) *.cs

clean:
	rm -f *.exe *.mdb 

cleanall:
	rm -f *.exe *.mdb *.zip *.txt *.ini

run: HitPlus.exe
	mono HitPlus.exe

cleanrun: cleanall run

icon.resources: icon.resx
	resgen.exe /compile icon.resx /r:System.Drawing
