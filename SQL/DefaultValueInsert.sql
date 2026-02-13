/* ============================================================
   Seed: Common Accessorials + EDI Charge Code mappings
   - Idempotent (safe to run multiple times)
   ============================================================ */

SET NOCOUNT ON;
GO

/* ----------------------------
   1) Accessorials
   ---------------------------- */
;WITH src AS (
    SELECT * FROM (VALUES
      (N'FUEL',        N'Fuel surcharge',                                  N'["LTL","FTL","FCL","LCL"]'),
      (N'LIFTGATE',    N'Liftgate service',                                N'["LTL","FTL"]'),
      (N'RES',         N'Residential pickup/delivery',                     N'["LTL","FTL"]'),
      (N'LIMITED',     N'Limited access pickup/delivery',                  N'["LTL","FTL"]'),
      (N'INSIDE',      N'Inside pickup/delivery',                          N'["LTL","FTL"]'),
      (N'APPT',        N'Appointment / delivery notification',             N'["LTL","FTL"]'),
      (N'CALL',        N'Call before delivery',                            N'["LTL","FTL"]'),
      (N'STOP_OFF',    N'Stop-off / stop charge',                          N'["LTL","FTL"]'),
      (N'DETENTION',   N'Detention',                                       N'["LTL","FTL","FCL","LCL"]'),
      (N'LUMPER',      N'Lumper / loading-unloading',                      N'["LTL","FTL"]'),
      (N'TONU',        N'Truck ordered not used (cancellation)',           N'["LTL","FTL"]'),
      (N'REDELIVERY',  N'Redelivery',                                      N'["LTL","FTL"]'),
      (N'RECONSIGN',   N'Reconsignment / change of destination',           N'["LTL","FTL"]'),
      (N'HAZMAT',      N'Hazardous materials surcharge',                   N'["LTL","FTL","FCL","LCL"]'),
      (N'OVERLENGTH',  N'Overlength / oversize piece surcharge',           N'["LTL","FTL"]'),
      (N'OVERSIZE',    N'Oversize handling surcharge',                     N'["LTL","FTL"]'),
      (N'SATURDAY',    N'Saturday pickup/delivery',                        N'["LTL","FTL"]'),
      (N'HOLIDAY',     N'Holiday pickup/delivery',                         N'["LTL","FTL"]'),
      (N'TOLLS',       N'Tolls / road fees',                               N'["FTL"]'),
      (N'BORDER',      N'Border crossing / customs processing',            N'["FTL","FCL","LCL"]'),
      (N'INSURANCE',   N'Cargo insurance',                                 N'["LTL","FTL","FCL","LCL"]'),

      /* Ocean / International common fees */
      (N'THC',         N'Terminal handling charge',                        N'["FCL","LCL"]'),
      (N'DOC',         N'Documentation fee',                               N'["FCL","LCL"]'),
      (N'ISPS',        N'ISPS security fee',                               N'["FCL","LCL"]'),
      (N'AMS',         N'Automated Manifest System (US)',                  N'["FCL","LCL"]'),
      (N'ENS',         N'Entry Summary Declaration (EU)',                  N'["FCL","LCL"]'),
      (N'CAF',         N'Currency adjustment factor',                      N'["FCL","LCL"]'),
      (N'BAF',         N'Bunker adjustment factor',                        N'["FCL","LCL"]'),
      (N'PSS',         N'Peak season surcharge',                           N'["FCL","LCL"]'),
      (N'DDC',         N'Destination delivery charge',                     N'["FCL","LCL"]'),
      (N'ODF',         N'Origin documentation fee',                        N'["FCL","LCL"]'),
      (N'DDF',         N'Destination documentation fee',                   N'["FCL","LCL"]'),
      (N'CFS',         N'CFS / devanning / handling',                      N'["LCL"]'),
      (N'CHASSIS',     N'Chassis fee',                                     N'["FCL"]'),
      (N'DEMURRAGE',   N'Demurrage',                                       N'["FCL","LCL"]'),
      (N'DETENTION_O', N'Container detention (ocean)',                     N'["FCL"]')
    ) v(Code, Description, ModeApplicabilityJson)
)
MERGE rating.Accessorials AS tgt
USING src
ON tgt.Code = src.Code
WHEN MATCHED THEN
  UPDATE SET
    tgt.Description = src.Description,
    tgt.ModeApplicabilityJson = src.ModeApplicabilityJson
WHEN NOT MATCHED THEN
  INSERT (Code, Description, ModeApplicabilityJson)
  VALUES (src.Code, src.Description, src.ModeApplicabilityJson);
GO


/* ----------------------------
   2) EDI Charge Codes (210/211/BOTH)
   - CanonicalChargeType: LINEHAUL, FUEL, ACCESSORIAL, DISCOUNT, TAX, OTHER
   - DefaultAccessorialCode links to rating.Accessorials.Code where relevant
   ---------------------------- */

;WITH src AS (
    SELECT * FROM (VALUES
      /* Core freight */
      (N'210', N'FRT',   N'Freight / Linehaul',                  N'LINEHAUL',    NULL),
      (N'BOTH',N'LH',    N'Linehaul',                            N'LINEHAUL',    NULL),
      (N'210', N'DSC',   N'Discount',                            N'DISCOUNT',    NULL),

      /* Fuel */
      (N'210', N'FSC',   N'Fuel surcharge',                      N'FUEL',        N'FUEL'),
      (N'BOTH',N'FUEL',  N'Fuel surcharge',                      N'FUEL',        N'FUEL'),

      /* Common LTL/FTL accessorials */
      (N'210', N'LFT',   N'Liftgate service',                    N'ACCESSORIAL', N'LIFTGATE'),
      (N'210', N'LIFT',  N'Liftgate service',                    N'ACCESSORIAL', N'LIFTGATE'),
      (N'210', N'RES',   N'Residential service',                 N'ACCESSORIAL', N'RES'),
      (N'210', N'LIN',   N'Limited access',                      N'ACCESSORIAL', N'LIMITED'),
      (N'210', N'INS',   N'Inside delivery',                     N'ACCESSORIAL', N'INSIDE'),
      (N'210', N'APPT',  N'Appointment',                         N'ACCESSORIAL', N'APPT'),
      (N'210', N'CALL',  N'Call before delivery',                N'ACCESSORIAL', N'CALL'),
      (N'210', N'STOP',  N'Stop-off charge',                     N'ACCESSORIAL', N'STOP_OFF'),
      (N'210', N'DET',   N'Detention',                           N'ACCESSORIAL', N'DETENTION'),
      (N'210', N'LUMP',  N'Lumper',                              N'ACCESSORIAL', N'LUMPER'),
      (N'210', N'TONU',  N'Truck ordered not used',              N'ACCESSORIAL', N'TONU'),
      (N'210', N'REDL',  N'Redelivery',                          N'ACCESSORIAL', N'REDELIVERY'),
      (N'210', N'RECN',  N'Reconsignment',                       N'ACCESSORIAL', N'RECONSIGN'),
      (N'210', N'HAZ',   N'Hazmat surcharge',                    N'ACCESSORIAL', N'HAZMAT'),
      (N'210', N'OVR',   N'Oversize/overlength',                 N'ACCESSORIAL', N'OVERSIZE'),
      (N'210', N'SAT',   N'Saturday service',                    N'ACCESSORIAL', N'SATURDAY'),
      (N'210', N'HOL',   N'Holiday service',                     N'ACCESSORIAL', N'HOLIDAY'),
      (N'210', N'TOLL',  N'Tolls / road fees',                   N'ACCESSORIAL', N'TOLLS'),
      (N'210', N'INSUR', N'Cargo insurance',                     N'ACCESSORIAL', N'INSURANCE'),

      /* Ocean / international common codes */
      (N'BOTH',N'THC',   N'Terminal handling charge',            N'ACCESSORIAL', N'THC'),
      (N'BOTH',N'DOC',   N'Documentation fee',                   N'ACCESSORIAL', N'DOC'),
      (N'BOTH',N'ISPS',  N'ISPS security fee',                   N'ACCESSORIAL', N'ISPS'),
      (N'BOTH',N'AMS',   N'AMS filing fee',                      N'ACCESSORIAL', N'AMS'),
      (N'BOTH',N'ENS',   N'ENS filing fee',                      N'ACCESSORIAL', N'ENS'),
      (N'BOTH',N'CAF',   N'Currency adjustment factor',          N'ACCESSORIAL', N'CAF'),
      (N'BOTH',N'BAF',   N'Bunker adjustment factor',            N'ACCESSORIAL', N'BAF'),
      (N'BOTH',N'PSS',   N'Peak season surcharge',               N'ACCESSORIAL', N'PSS'),
      (N'BOTH',N'DDC',   N'Destination delivery charge',         N'ACCESSORIAL', N'DDC'),
      (N'BOTH',N'ODF',   N'Origin documentation fee',            N'ACCESSORIAL', N'ODF'),
      (N'BOTH',N'DDF',   N'Destination documentation fee',       N'ACCESSORIAL', N'DDF'),
      (N'BOTH',N'CFS',   N'CFS/devanning/handling',              N'ACCESSORIAL', N'CFS'),
      (N'BOTH',N'CHAS',  N'Chassis fee',                         N'ACCESSORIAL', N'CHASSIS'),
      (N'BOTH',N'DEM',   N'Demurrage',                           N'ACCESSORIAL', N'DEMURRAGE'),
      (N'BOTH',N'DETC',  N'Container detention',                 N'ACCESSORIAL', N'DETENTION_O'),

      /* Taxes/other (optional) */
      (N'BOTH',N'TAX',   N'Tax',                                 N'TAX',         NULL),
      (N'BOTH',N'OTH',   N'Other charge',                        N'OTHER',       NULL)
    ) v(Standard, Code, Description, CanonicalChargeType, DefaultAccessorialCode)
)
MERGE rating.EdiChargeCodes AS tgt
USING src
ON tgt.Standard = src.Standard AND tgt.Code = src.Code
WHEN MATCHED THEN
  UPDATE SET
    tgt.Description = src.Description,
    tgt.CanonicalChargeType = src.CanonicalChargeType,
    tgt.DefaultAccessorialCode = src.DefaultAccessorialCode
WHEN NOT MATCHED THEN
  INSERT (Standard, Code, Description, CanonicalChargeType, DefaultAccessorialCode)
  VALUES (src.Standard, src.Code, src.Description, src.CanonicalChargeType, src.DefaultAccessorialCode);
GO
