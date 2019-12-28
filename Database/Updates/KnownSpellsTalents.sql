SET FOREIGN_KEY_CHECKS=0;

-- -----------------------------------------------------
-- Table `knownspells`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `knownspells`;
CREATE TABLE `knownspells` (
  `spell` MEDIUMINT(8) UNSIGNED NOT NULL DEFAULT 0,
  `guid` INT(11) UNSIGNED NOT NULL DEFAULT 0,
  PRIMARY KEY (`spell`, `guid`),
  INDEX `fk_knownspells_characters1_idx` (`guid` ASC),
  CONSTRAINT `fk_knownspells_characters1`
    FOREIGN KEY (`guid`)
    REFERENCES `characters` (`guid`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;


-- -----------------------------------------------------
-- Table `knowntalents`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `knowntalents`;
CREATE TABLE `knowntalents` (
  `talent` MEDIUMINT(8) UNSIGNED NOT NULL DEFAULT 0,
  `guid` INT(11) UNSIGNED NOT NULL DEFAULT 0,
  PRIMARY KEY (`talent`, `guid`),
  INDEX `fk_knowntalents_characters1_idx` (`guid` ASC),
  CONSTRAINT `fk_knowntalents_characters1`
    FOREIGN KEY (`guid`)
    REFERENCES `characters` (`guid`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

SET FOREIGN_KEY_CHECKS=1;
