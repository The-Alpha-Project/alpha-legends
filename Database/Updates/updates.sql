-- 10/09/2019
-- Changing displayids for the correct ones
update item_template set displayid = 7601 where entry = 1251; -- Linen Bandage
update item_template set displayid = 7588 where entry = 2581; -- Heavy Linen Bandage
update item_template set displayid = 9430 where entry = 3530; -- Wool Bandage
update item_template set displayid = 7598 where entry = 3531; -- Heavy Wool Bandage

-- 12/09/2019
-- Fix broken Thunder Bluff elevators
-- TODO: 4171 and 4173 are still a bit bugged.
update gameobjects set entry = 4173 where entry = 4171;
update gameobjects set entry = 4171 where entry = 47296;
update gameobjects set entry = 4172 where entry = 47297;
