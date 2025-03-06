# Changelog

## [0.0.10] - 2024-12-27

### Added

- Populate Object 时考虑之前定义的 Converter

## [0.0.9] - 2024-04-03

### Added

- 支持在进行 SerializeObject 时传入 JsonSerializerSettings

## [0.0.8] - 2024-03-15

### Added

- 适配最新的 Utilities 0.15.0 版本

## [0.0.7] - 2023-09-21

### Fixed

- 修复当目标类型为 bool 时，tryDeserializeObject 方法失败的问题

## [0.0.6] - 2023-07-06

### Fixed

- 修复当目标字段中包含有特殊字符时，解序列化失败的问题

## [0.0.5] - 2023-06-27

### Fixed

- 修复当目标类型为 string 时，tryDeserializeObject 方法失败的问题

## [0.0.4] - 2023-02-21

### Added

- 增加 IsValidJson 接口用以判断字符串是否是 Json

## [0.0.3] - 2023-01-17

### Fixed

- 修复 DateTimeConverter 在解析错误的时机戳时异常

## [0.0.2] - 2022-11-07

### Fixed

- 修复 Meta 文件冲突问题

## [0.0.1] - 2022-11-07

### Added
- 初始化工程
