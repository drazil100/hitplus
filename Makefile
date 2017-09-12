ifeq ($(OS),Windows_NT)
	CSC = csc
	RM = del
else
	CSC = mcs
	RM = rm -f
endif

all: SF64ScoreTracker.exe

SF64ScoreTracker.exe: $(wildcard *.cs)
	$(CSC) /out:SF64ScoreTracker.exe *.cs

clean:
	$(RM) *.exe *.mdb 

zip: 
	rm -f /home/austin/public_html/irc.zip
	zip -9 /home/austin/public_html/irc.zip *.cs
