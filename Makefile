ifeq ($(OS),Windows_NT)
	CSC = csc
	RM = del
	REFERENCES =
else
	CSC = mcs
	RM = rm -f
	REFERENCES = -r:System.Windows.Forms,System.Drawing
endif

all: SF64ScoreTracker.exe

SF64ScoreTracker.exe: $(wildcard *.cs)
	$(CSC) /out:SF64ScoreTracker.exe /win32icon:medal.ico $(REFERENCES) *.cs www/*.cs

clean:
	$(RM) *.exe *.mdb

icon.resources: icon.resx
	resgen.exe /compile icon.resx /r:System.Drawing
