CREATE TABLE smtpServer(
	idSmtp INTEGER PRIMARY KEY NOT NULL,
	smtpName VARCHAR(60) NOT NULL,
	smtpHost VARCHAR(80) NOT NULL,
	smtpPort INT NOT NULL DEFAULT 587,
	smtpEncrypt BOOLEAN NOT NULL DEFAULT true,
	smtpFrom VARCHAR(150) NOT NULL,
	smtpUser VARCHAR(100) NOT NULL,
	smtpPass VARCHAR(40) NOT NULL
);