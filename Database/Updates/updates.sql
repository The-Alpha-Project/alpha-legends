delimiter $
begin not atomic
	-- 26/09/2019 2
	if (select count(*) from applied_updates where id='260920192') = 0 then
		update item_template set display_id = 926 where entry = 8766;
		update item_template set display_id = 2209 where entry = 17184;

		insert into applied_updates values('260920192');
	end if;

	-- 26/09/2019 3
	if (select count(*) from applied_updates where id='260920193') = 0 then
		update item_template set display_id = 6639 where entry = 6276;
		update item_template set display_id = 2702 where entry = 2481;
		update item_template set display_id = 2380 where entry = 2128;
		update item_template set display_id = 8479 where entry = 2482;
		update item_template set display_id = 8515 where entry = 2483;
		update item_template set display_id = 5219 where entry = 2485;
		update item_template set display_id = 4423 where entry = 2486;
		update item_template set display_id = 2388 where entry = 2487;
		update item_template set display_id = 8479 where entry = 2398;
		update item_template set display_id = 2398 where entry = 2496;
		update item_template set display_id = 2404 where entry = 2503;
		update item_template set display_id = 8576 where entry = 2500;
		update item_template set display_id = 6444 where entry = 2502;
		update item_template set display_id = 2399 where entry = 2497;
		update item_template set display_id = 7414 where entry = 6216;
		update item_template set display_id = 8687 where entry = 2501;

		update item_template set name='Forsaken Mace', display_id = 5206 where entry = 3269;
		update item_template set name='Runic Cloth Cloak', armor=25, display_id = 4613, quality=2, buy_price=1012, sell_price=22, required_level=10, item_level = 15, stat_type1=6, stat_value1=1 where entry = 4686;
		update item_template set display_id = 6639 where entry = 6276;
		update item_template set name='Inscribed Leather Cloak', buy_price=1579, sell_price=315, stat_type1=1, stat_value1=10, armor=50, display_id = 8792 where entry = 4701;
		update item_template set name='Runic Cloth Belt', buy_price=677, sell_price=135, stat_type1=5, stat_value1=1, required_level=10, item_level=15 where entry = 4687;
		update item_template set name='Runic Cloth Gloves', display_id=11423, required_level=11, buy_price=829, sell_price=165, stat_type1=5, stat_value1=1, stat_type2=3, stat_value2=1, stat_type3=0, stat_value3=0, armor=20 where entry = 3308;
		update item_template set name='Buckled Cloth Trousers', display_id=3731, armor=12 where entry=3834;
		update item_template set armor=257, block=6, stat_type1=4, stat_value1=1, stat_type2=7, stat_value2=2, required_level=12, item_level=16, display_id=3931, buy_price=2911, sell_price=582 where entry = 3654;
		update item_template set name='Sturdy Flail', display_id=5197 where entry = 852;
		update item_template set display_id=2632, armor=114, block=2, required_level=5, item_level=10, sell_price=88, buy_price=342 where entry = 3650;
		update item_template set display_id=11424, name='Runic Cloth Bracers', item_level=16, required_level=12, armor=18, buy_price=566, sell_price=113, armor=18 where entry = 3644;
		update item_template set name='Runic Cloth Vest', display_id=11419, armor=33, item_level=15, required_level=10, buy_price=1454, sell_price=290, stat_type1=7, stat_value1=1, stat_type2=6, stat_value2=3 where entry = 3310;
		update item_template set name='Large Broad Axe', display_id=8524 where entry = 1196;

		insert into applied_updates values('260920193');
	end if;
end $
delimiter ;

