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

	-- 13/09/2019 1
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

		-- Correct stats for Small Shield
		update item_template set buyprice = 35, sellprice = 6, itemlevel = 3, armor = 29, maxdurability = 35 where entry = 2133;

		-- Using the correct alpha Small Shield
		set foreign_key_checks = 0;
		update npc_vendor set item = 2133 where item = 17184;
		update creature_loot_template set item = 2133 where item = 17184;
		set foreign_key_checks = 1;
		delete from item_template where entry = 17184;

		insert into applied_updates values ('130920191');
	end if;

	-- 13/09/2019 2
	if (select count(*) from applied_updates where id='130920192') = 0 then
		delete from spawns_gameobjects where spawn_id = 26727;
		delete from spawns_gameobjects where spawn_id = 26728;
		delete from spawns_gameobjects where spawn_id = 26740;

		insert into worldports (x, y, z, map, name) values (3167.27, -1407.15, 0, 17, 'Kalidar');
		insert into worldports (x, y, z, map, name) values (0, 0, 0, 29, 'Cage');
		insert into worldports (x, y, z, map, name) values (100, 100, 200, 30, 'PvPZone01');
		insert into worldports (x, y, z, map, name) values (277.77, -888.38, 400, 37, 'PvPZone02');
		insert into worldports (x, y, z, map, name) values (0.76, -0.91, -2.32, 44, 'OldScarletMonastery');

		insert into applied_updates values ('130920192');
	end if;

	-- 14/09/2019 1
	if (select count(*) from applied_updates where id='140920191') = 0 then
		update creature_model_info set modelid = 3831 where modelid = 10699;

		insert into applied_updates values ('140920191');
	end if;

	-- 15/09/2019 1
	if (select count(*) from applied_updates where id='150920191') = 0 then
		update creatures set modelid = 3166 where modelid = 1918;
		update creatures set modelid = 1545 where entry = 2612;
		update creatures set modelid = 1545 where entry = 2738;
		update creatures set modelid = 197, minlevel = 8, maxlevel = 8 where entry = 197;
		update creatures set minlevel = 5, maxlevel = 5 where entry = 823;
		update creatures set modelid = 164 where entry in (1642, 823);

		update spawns_creatures set spawn_positionx = -8933.54, spawn_positiony = -136.523, spawn_positionz = 83.4466, spawn_orientation = 1.97222 where spawn_id = 79970;
		update spawns_creatures set spawn_positionx = -8924.164, spawn_positiony = -136.1524, spawn_positionz = 81.0561, spawn_orientation = 2.192002 where spawn_id = 79942;

		update item_template set displayid = 292 where displayid = 15710;

		update quests set details = "Kobolds have infested the woods to the north. If you kill 7 of the grimy Vermin for me, then you'll have my thanks, citizen.", objectives = "Kill 7 Kobold Vermin, then return to Marshal McBride.", 
			ReqCreatureOrGOCount1 = 7, RewOrReqMoney = 20, RewItemId1 = 118 where entry = 7;

		insert into applied_updates values ('150920191');
	end if;
end $
delimiter ;