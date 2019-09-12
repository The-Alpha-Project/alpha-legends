create table if not exists applied_updates
(
	id varchar(9) not null primary key default '000000000'
);

delimiter $
begin not atomic
	-- 10/09/2019
	if (select count(*) from applied_updates where id='100920191') = 0 then
		-- Changing displayids for the correct ones
		update item_template set displayid = 7601 where entry = 1251; -- Linen Bandage
		update item_template set displayid = 7588 where entry = 2581; -- Heavy Linen Bandage
		update item_template set displayid = 9430 where entry = 3530; -- Wool Bandage
		update item_template set displayid = 7598 where entry = 3531; -- Heavy Wool Bandage

		insert into applied_updates values ('100920191');
	end if;

	-- 12/09/2019
	if (select count(*) from applied_updates where id='120920191') = 0 then
		-- Fix broken Thunder Bluff elevators
		-- TODO: 4171 and 4173 are still a bit bugged.
		update gameobjects set entry = 4173 where entry = 4171;
		update gameobjects set entry = 4171 where entry = 47296;
		update gameobjects set entry = 4172 where entry = 47297; 

		insert into applied_updates values ('120920191');
	end if;
end $
delimiter ;