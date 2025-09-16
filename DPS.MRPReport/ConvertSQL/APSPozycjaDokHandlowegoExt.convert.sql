-- versionname:DPSMRPReport version:9999

INSERT INTO [APSPozDHExt] ([PozycjaDokHandlowego])
SELECT [ID] FROM [PozycjeDokHan]
WHERE NOT EXISTS (SELECT NULL
					FROM [APSPozDHExt]
					WHERE [APSPozDHExt].[PozycjaDokHandlowego] = [PozycjeDokHan].[ID])