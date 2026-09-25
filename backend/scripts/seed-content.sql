-- Idempotent local/demo content seed. Run with sqlplus against FREEPDB1.
-- Product images and visit content are intentionally API-owned data, not UI fixtures.
set define off
set serveroutput on

merge into "COSMICWIKI_PRODUCT"."CATEGORIES" c
using (select hextoraw('10000000000000000000000000000001') id, 'Cosmic artifacts' name, 'cosmic-artifacts' slug, 'Museum artifacts' description from dual) s
on (c.id = s.id)
when not matched then insert (id,name,slug,description) values (s.id,s.name,s.slug,s.description);

merge into "COSMICWIKI_PRODUCT"."CATEGORIES" c
using (select hextoraw('10000000000000000000000000000002') id, 'Astronomical instruments' name, 'astronomical-instruments' slug, 'Historic instruments' description from dual) s
on (c.id = s.id)
when not matched then insert (id,name,slug,description) values (s.id,s.name,s.slug,s.description);

merge into "COSMICWIKI_PRODUCT"."PRODUCTS" p
using (select hextoraw('20000000000000000000000000000001') id, 'GB-4500' sku, 'Gibeon Meteorite Fragment' name, 'gibeon-meteorite-fragment' slug, 3850000 price, 12 stock, hextoraw('10000000000000000000000000000001') category_id from dual) s
on (p.id = s.id)
when not matched then insert (id,sku,name,slug,description,original_price,sale_price,stock_quantity,status,category_id)
values (s.id,s.sku,s.name,s.slug,'Iron-nickel meteorite fragment from Namibia.',s.price,s.price,s.stock,2,s.category_id);

merge into "COSMICWIKI_PRODUCT"."PRODUCTS" p
using (select hextoraw('20000000000000000000000000000002') id, 'GL-1610' sku, 'Galileo Star Map Reproduction' name, 'galileo-star-map-reproduction' slug, 1250000 price, 20 stock, hextoraw('10000000000000000000000000000001') category_id from dual) s
on (p.id = s.id)
when not matched then insert (id,sku,name,slug,description,original_price,sale_price,stock_quantity,status,category_id)
values (s.id,s.sku,s.name,s.slug,'Archival reproduction of the 1610 astronomical map.',s.price,s.price,s.stock,2,s.category_id);

merge into "COSMICWIKI_PRODUCT"."PRODUCTS" p
using (select hextoraw('20000000000000000000000000000003') id, 'AST-1540' sku, 'Brass Astrolabe' name, 'brass-astrolabe' slug, 5400000 price, 8 stock, hextoraw('10000000000000000000000000000002') category_id from dual) s
on (p.id = s.id)
when not matched then insert (id,sku,name,slug,description,original_price,sale_price,stock_quantity,status,category_id)
values (s.id,s.sku,s.name,s.slug,'Brass astronomical navigation instrument reproduction.',s.price,s.price,s.stock,2,s.category_id);

merge into "COSMICWIKI_PRODUCT"."PRODUCTS" p
using (select hextoraw('20000000000000000000000000000004') id, 'ARM-1720' sku, 'Armillary Sphere' name, 'armillary-sphere' slug, 4200000 price, 6 stock, hextoraw('10000000000000000000000000000002') category_id from dual) s
on (p.id = s.id)
when not matched then insert (id,sku,name,slug,description,original_price,sale_price,stock_quantity,status,category_id)
values (s.id,s.sku,s.name,s.slug,'Gold-finished armillary sphere for the museum collection.',s.price,s.price,s.stock,2,s.category_id);

merge into "COSMICWIKI_PRODUCT"."PRODUCT_IMAGES" i
using (select hextoraw('21000000000000000000000000000001') id, hextoraw('20000000000000000000000000000001') product_id, 'https://lh3.googleusercontent.com/aida-public/AB6AXuCPGZmvjgpmLd2Kxhwplzvo1hp1XOjJXd7hsmuGAfKwBEn8SMWEnDrSahVfln0OumbqHSKvj6yvooVkrJxXvTEgdK-bWRD1rMVRYS103HxuI02NBifurGsBGZ1dyfNEr4MmQBF7zmfqic51ilOLA5x1WXTJx6shD4xQ1ob5PCDLbIJB-yr_SOte04ERcYfn-rRk2d_Kxu2NRBHvdvvEmhx-ly8FAAzXjzayw0lmO9AplnD6_9UOzeiCkns6kKZHmsD_S3wLFplMJw' url, 0 display_order, '["https://lh3.googleusercontent.com/aida-public/AB6AXuCPGZmvjgpmLd2Kxhwplzvo1hp1XOjJXd7hsmuGAfKwBEn8SMWEnDrSahVfln0OumbqHSKvj6yvooVkrJxXvTEgdK-bWRD1rMVRYS103HxuI02NBifurGsBGZ1dyfNEr4MmQBF7zmfqic51ilOLA5x1WXTJx6shD4xQ1ob5PCDLbIJB-yr_SOte04ERcYfn-rRk2d_Kxu2NRBHvdvvEmhx-ly8FAAzXjzayw0lmO9AplnD6_9UOzeiCkns6kKZHmsD_S3wLFplMJw"]' urls from dual) s
on (i.id=s.id)
when not matched then insert (id,product_id,url,display_order,urls) values (s.id,s.product_id,s.url,s.display_order,s.urls);

merge into "COSMICWIKI_PRODUCT"."PRODUCT_IMAGES" i
using (select hextoraw('21000000000000000000000000000002') id, hextoraw('20000000000000000000000000000002') product_id, 'https://lh3.googleusercontent.com/aida-public/AB6AXuCIBZEOrc8zSK-2kWGkZIKeX1pfKmGeVNGlAD0PlBZz6CHcIRay8ovboxjAmpyQq4NnFzCe9eaOH_5gxisC6efIbVoXTb2uQvwxDtp9-IRIhbuISMGmRpDt-lqd_dA6rY9fBXbJakqYYWojg3BNBKCYq3MZFmgLQ_yIKZMcAbCPldvSPtEFKBtqeIFK08Fb5dBKOBvJ2IXmTmg3jUJME3I2odYyhWEBuWKsIpUcKQ4XGCMKfGRKVusRh_LomSoftuQbLnrAq8Wr2Q' url, 0 display_order from dual) s
on (i.id=s.id)
when not matched then insert (id,product_id,url,display_order,urls) values (s.id,s.product_id,s.url,s.display_order,'["' || s.url || '"]');

merge into "COSMICWIKI_PRODUCT"."PRODUCT_IMAGES" i
using (select hextoraw('21000000000000000000000000000003') id, hextoraw('20000000000000000000000000000003') product_id, 'https://lh3.googleusercontent.com/aida-public/AB6AXuDq-K8VzkxDDZNzJZ8glN7T1Y9XC5Mxo9EgwGafiCXEl6wKwG4En5373g-v8eR_ofDKDVoJk3eGzy4deezIsjCKhiRgsjoWA8P7LgPyQvlZkItiqjAptNex2Wn5bKUfr10o-EL9PUUNMaI9GBmFuWitk4VyXLh1wP7akpCb89VCtfLVi-_8xJ8Plx-lq1PnXl_Hxrs0984YmnwUVbVB9t-P_Du1ee_bXnwIGoFIoQps-WJ9Xa0x1S572nverUnOHsE4nM_Wi1SEcg' url, 0 display_order from dual) s
on (i.id=s.id)
when not matched then insert (id,product_id,url,display_order,urls) values (s.id,s.product_id,s.url,s.display_order,'["' || s.url || '"]');

merge into "COSMICWIKI_PRODUCT"."PRODUCT_IMAGES" i
using (select hextoraw('21000000000000000000000000000004') id, hextoraw('20000000000000000000000000000004') product_id, 'https://lh3.googleusercontent.com/aida-public/AB6AXuAP14M9pQ1MLcN2wOxZ6H2VCeo7rDJk4o3VHdbeFs0tQ3wLTXDtYDEL04QnY93ZXcHzGD2bdbrpUm8cyUvUqqfsul7_tdYefWUVG_S7tOMeOirS59rKkpaoAPlZgz7gmZRPZPmuVv9gwAKU-2uA2wDR9FOwe4_4z65WFLnwVRyryOoOhfu6sBemjh9FVMlZetDNCTGiHLBqMbfylU0aGgf01VhvrRDtFRjCunbKRX0AhX21pRWGzF7gKqpXo-FFDggKazRN6pB8mw' url, 0 display_order from dual) s
on (i.id=s.id)
when not matched then insert (id,product_id,url,display_order,urls) values (s.id,s.product_id,s.url,s.display_order,'["' || s.url || '"]');

merge into "COSMICWIKI_PRODUCT"."PRODUCT_IMAGES" i
using (select hextoraw('21000000000000000000000000000011') id, hextoraw('20000000000000000000000000000001') product_id, 'https://lh3.googleusercontent.com/aida-public/AB6AXuCPGZmvjgpmLd2Kxhwplzvo1hp1XOjJXd7hsmuGAfKwBEn8SMWEnDrSahVfln0OumbqHSKvj6yvooVkrJxXvTEgdK-bWRD1rMVRYS103HxuI02NBifurGsBGZ1dyfNEr4MmQBF7zmfqic51ilOLA5x1WXTJx6shD4xQ1ob5PCDLbIJB-yr_SOte04ERcYfn-rRk2d_Kxu2NRBHvdvvEmhx-ly8FAAzXjzayw0lmO9AplnD6_9UOzeiCkns6kKZHmsD_S3wLFplMJw' url, 1 display_order from dual) s
on (i.id=s.id)
when not matched then insert (id,product_id,url,display_order,urls) values (s.id,s.product_id,s.url,s.display_order,'["' || s.url || '"]');

merge into "COSMICWIKI_PRODUCT"."PRODUCT_IMAGES" i
using (select hextoraw('21000000000000000000000000000012') id, hextoraw('20000000000000000000000000000002') product_id, 'https://lh3.googleusercontent.com/aida-public/AB6AXuCIBZEOrc8zSK-2kWGkZIKeX1pfKmGeVNGlAD0PlBZz6CHcIRay8ovboxjAmpyQq4NnFzCe9eaOH_5gxisC6efIbVoXTb2uQvwxDtp9-IRIhbuISMGmRpDt-lqd_dA6rY9fBXbJakqYYWojg3BNBKCYq3MZFmgLQ_yIKZMcAbCPldvSPtEFKBtqeIFK08Fb5dBKOBvJ2IXmTmg3jUJME3I2odYyhWEBuWKsIpUcKQ4XGCMKfGRKVusRh_LomSoftuQbLnrAq8Wr2Q' url, 1 display_order from dual) s
on (i.id=s.id)
when not matched then insert (id,product_id,url,display_order,urls) values (s.id,s.product_id,s.url,s.display_order,'["' || s.url || '"]');

merge into "COSMICWIKI_PRODUCT"."PRODUCT_IMAGES" i
using (select hextoraw('21000000000000000000000000000013') id, hextoraw('20000000000000000000000000000003') product_id, 'https://lh3.googleusercontent.com/aida-public/AB6AXuDq-K8VzkxDDZNzJZ8glN7T1Y9XC5Mxo9EgwGafiCXEl6wKwG4En5373g-v8eR_ofDKDVoJk3eGzy4deezIsjCKhiRgsjoWA8P7LgPyQvlZkItiqjAptNex2Wn5bKUfr10o-EL9PUUNMaI9GBmFuWitk4VyXLh1wP7akpCb89VCtfLVi-_8xJ8Plx-lq1PnXl_Hxrs0984YmnwUVbVB9t-P_Du1ee_bXnwIGoFIoQps-WJ9Xa0x1S572nverUnOHsE4nM_Wi1SEcg' url, 1 display_order from dual) s
on (i.id=s.id)
when not matched then insert (id,product_id,url,display_order,urls) values (s.id,s.product_id,s.url,s.display_order,'["' || s.url || '"]');

merge into "COSMICWIKI_PRODUCT"."PRODUCT_IMAGES" i
using (select hextoraw('21000000000000000000000000000014') id, hextoraw('20000000000000000000000000000004') product_id, 'https://lh3.googleusercontent.com/aida-public/AB6AXuAP14M9pQ1MLcN2wOxZ6H2VCeo7rDJk4o3VHdbeFs0tQ3wLTXDtYDEL04QnY93ZXcHzGD2bdbrpUm8cyUvUqqfsul7_tdYefWUVG_S7tOMeOirS59rKkpaoAPlZgz7gmZRPZPmuVv9gwAKU-2uA2wDR9FOwe4_4z65WFLnwVRyryOoOhfu6sBemjh9FVMlZetDNCTGiHLBqMbfylU0aGgf01VhvrRDtFRjCunbKRX0AhX21pRWGzF7gKqpXo-FFDggKazRN6pB8mw' url, 1 display_order from dual) s
on (i.id=s.id)
when not matched then insert (id,product_id,url,display_order,urls) values (s.id,s.product_id,s.url,s.display_order,'["' || s.url || '"]');

commit;
