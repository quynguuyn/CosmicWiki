-- Run as SYSDBA against FREEPDB1 after the Visit migration.
alter session set container=FREEPDB1;

merge into "COSMICWIKI_VISIT"."EXHIBITIONS" e using (select hextoraw('30000000000000000000000000000001') id, '01' number_value, 'Planetary systems' category, 'Solar System Hall' title, 'Interactive orbital models and lunar terrain scans.' description, '3D interactive' status, 1 display_order from dual) s on (e.id=s.id)
when not matched then insert (id,"NUMBER",category,title,description,status,image_url,display_order,is_published,created_at_utc,updated_at_utc)
values (s.id,s.number_value,s.category,s.title,s.description,s.status,null,s.display_order,true,systimestamp,systimestamp);

merge into "COSMICWIKI_VISIT"."EXHIBITIONS" e using (select hextoraw('30000000000000000000000000000002') id, '02' number_value, 'Deep space' category, 'Galaxy and Black Hole Garden' title, 'Infrared datasets from JWST and the Event Horizon Telescope.' description, 'JWST data' status, 2 display_order from dual) s on (e.id=s.id)
when not matched then insert (id,"NUMBER",category,title,description,status,image_url,display_order,is_published,created_at_utc,updated_at_utc)
values (s.id,s.number_value,s.category,s.title,s.description,s.status,null,s.display_order,true,systimestamp,systimestamp);

merge into "COSMICWIKI_VISIT"."FAQS" f using (select hextoraw('40000000000000000000000000000001') id, 'How can I access raw FITS data?' question, 'Processed observation files are available through the CosmicWiki research data portal.' answer, 1 display_order from dual) s on (f.id=s.id)
when not matched then insert (id,question,answer,display_order,is_published,created_at_utc,updated_at_utc)
values (s.id,s.question,s.answer,s.display_order,true,systimestamp,systimestamp);

merge into "COSMICWIKI_VISIT"."EVENTS" e using (select hextoraw('50000000000000000000000000000001') id, systimestamp + interval '14' day event_date_utc, 'Perseids meteor shower' name, 'Live observation session from the observatory.' description, 'Peak activity' specification, 'North-east sky' location, 'Registration open' status, 'primary' status_type, 'Register' action, 1 display_order from dual) s on (e.id=s.id)
when not matched then insert (id,event_date_utc,name,description,specification,location,status,status_type,action,display_order,is_published,created_at_utc,updated_at_utc)
values (s.id,s.event_date_utc,s.name,s.description,s.specification,s.location,s.status,s.status_type,s.action,s.display_order,true,systimestamp,systimestamp);

commit;
