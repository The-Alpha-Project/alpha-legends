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

	-- 13/09/2019
	if (select count(*) from applied_updates where id='130920191') = 0 then
		-- "Ticket" system
		create table if not exists tickets
		(
			id int(11) not null primary key auto_increment,
			is_bug int(1) not null default 0,
			account_name varchar(250) not null default '',
			account_id int(10) unsigned not null default 0,
			character_name varchar(12) not null default '',
			text_body text not null default '',
			submit_time timestamp default current_timestamp on update current_timestamp,

			foreign key (account_id) references accounts(id) on delete cascade on update cascade
		);

		-- Using the correct alpha Small Shield
		set foreign_key_checks = 0;
		update npc_vendor set item = 2133 where item = 17184;
		update creature_loot_template set item = 2133 where item = 17184;
		set foreign_key_checks = 1;
		delete from item_template where entry = 17184;

		insert into applied_updates values ('130920191');
	end if;
end $
delimiter ;