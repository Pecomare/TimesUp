-- DROP ALL --

DROP TABLE IF EXISTS Card;
DROP TABLE IF EXISTS Deck;

-- Deck --

CREATE TABLE Deck(Id SERIAL PRIMARY KEY, Name VARCHAR(64));

INSERT INTO Deck(Name) VALUES ('Toto'), ('Titi');

-- Card --

CREATE TABLE Card(
	Id SERIAL PRIMARY KEY
	, DeckId int
	, Text1 VARCHAR(64)
	, Text2 VARCHAR(64)
	, CONSTRAINT FK_Deck_Id FOREIGN KEY (DeckId) REFERENCES Deck (Id)
		ON DELETE CASCADE
		ON UPDATE CASCADE);

INSERT INTO Card(DeckId, Text1, Text2) VALUES (1, '1', '2'), (1, '3', '4'), (1, '5', '6'), (1, '7', '8'), (1, '9', '0');

-- ? --
