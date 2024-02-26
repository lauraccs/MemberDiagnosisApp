


CREATE PROCEDURE MemberDiagnosisStoredProc (@MemberID INT) AS
BEGIN
 ;WITH cte AS ( 
    SELECT DISTINCT 
    m.MemberID, m.FirstName, m.LastName,
       MIN(md.DiagnosisID) OVER (PARTITION BY dcm.DiagnosisCategoryID) AS 'Min Diagnosis', 
       dc.CategoryDescription,
        dcm.DiagnosisCategoryID,
        dc.CategoryScore
FROM   Member m
LEFT JOIN MemberDiagnosis md on md.MemberID = m.MemberID
LEFT JOIN DiagnosisCategoryMap dcm on dcm.DiagnosisID = md.DiagnosisID
LEFT JOIN Diagnosis d on d.DiagnosisID = dcm.DiagnosisID
LEFT JOIN DiagnosisCategory dc on dc.DiagnosisCategoryID = dcm.DiagnosisCategoryID
WHERE m.MemberID = @MemberID
GROUP BY m.MemberID, m.FirstName, m.LastName, md.DiagnosisID, dcm.DiagnosisCategoryID, dc.CategoryDescription, md.MemberID, dcm.DiagnosisID, dc.CategoryScore
)
  SELECT b.MemberID, c.FirstName, c.LastName, 
  c.[Min Diagnosis] AS 'Most Severe Diagnosis ID', 
  d.DiagnosisDescription AS 'Most Severe Diagnosis Description', 
  c.DiagnosisCategoryID AS 'CategoryID', 
  c.CategoryDescription,
  c.CategoryScore,
  CAST(CASE WHEN b.MostSevereCategory = c.DiagnosisCategoryID or b.MostSevereCategory is null
                THEN 1
                ELSE 0
                END 
                AS BIT) AS 'IsMostSevereCategory'  
FROM cte AS c
    RIGHT JOIN
        (SELECT MemberID, min(DiagnosisCategoryID) as 'MostSevereCategory'
        FROM cte
        GROUP BY MemberID) AS b
        ON c.MemberID = b.MemberID
    LEFT JOIN Diagnosis d on d.DiagnosisID = c.[Min Diagnosis];

END