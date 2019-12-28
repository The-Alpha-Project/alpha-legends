/*
Navicat MariaDB Data Transfer

Source Server         : MariaDB
Source Server Version : 100411
Source Host           : localhost:3306
Source Database       : alpha_world

Target Server Type    : MariaDB
Target Server Version : 100411
File Encoding         : 65001

Date: 2019-12-28 09:14:10
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for knownspells
-- ----------------------------
DROP TABLE IF EXISTS `knownspells`;
CREATE TABLE `knownspells` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `guid` int(11) unsigned NOT NULL DEFAULT 0 COMMENT 'Global Unique Identifier',
  `Spell` mediumint(8) unsigned NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1360 DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of knownspells
-- ----------------------------

-- ----------------------------
-- Table structure for knowntalents
-- ----------------------------
DROP TABLE IF EXISTS `knowntalents`;
CREATE TABLE `knowntalents` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `guid` int(11) unsigned NOT NULL DEFAULT 0 COMMENT 'Global Unique Identifier',
  `Talent` mediumint(8) unsigned NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1354 DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records of knowntalents
-- ----------------------------
SET FOREIGN_KEY_CHECKS=1;
