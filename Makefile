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
	$(CSC) /out:SF64ScoreTracker.exe $(REFERENCES) *.cs

clean:
	$(RM) *.exe *.mdb

zip:
	rm -f /home/austin/public_html/irc.zip
	zip -9 /home/austin/public_html/irc.zip *.cs
