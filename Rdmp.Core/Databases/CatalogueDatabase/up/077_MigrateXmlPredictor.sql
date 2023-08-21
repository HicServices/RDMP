---Version:8.1.0
--Description: Migrate ValuePredictsOtherValueNullness Predictor to ValuePredictsOtherValueNullity
BEGIN
UPDATE Catalogue
SET ValidatorXML = REPLACE(ValidatorXML, 'ValuePredictsOtherValueNullness','ValuePredictsOtherValueNullity')
WHERE ValidatorXML IS NOT NULL
END
