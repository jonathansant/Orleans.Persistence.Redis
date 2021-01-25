## [1.0.3](https://github.com/jonathansant/orleans.persistence.redis/compare/1.0.2...1.0.3) (2021-01-25)

### Features

- Add timer diagnostic which takes long

## [1.0.2](https://github.com/jonathansant/orleans.persistence.redis/compare/1.0.1...1.0.2) (2021-01-25)

### Features

- Improve the diagnostic message and include key

## [1.0.1](https://github.com/jonathansant/orleans.persistence.redis/compare/1.0.0...1.0.1) (2021-01-15)

### Features

- Improve type name logging

## [1.0.0](https://github.com/jonathansant/orleans.persistence.redis/compare/0.8.1...1.0.0) (2021-01-15)

### Features

- Semantic Versioning
- Improve the diagnostic message when state exceeds threshold size

## [0.8.1](https://github.com/jonathansant/orleans.persistence.redis/compare/0.8.0...0.8.1) (2021-01-14)

### Bug Fixes

- Fix log diagnostic message when state exceeds threshold size

## [0.8.0](https://github.com/jonathansant/orleans.persistence.redis/compare/0.7.0...0.8.0) (2021-01-14)

### Features

- Log diagnostic message when state exceeds threshold size

## [0.7.0](https://github.com/jonathansant/orleans.persistence.redis/compare/0.6.0...0.7.0) (2019-06-23)

### Features

- Update Orleans v3.2 & Redis v2.1.58

### BREAKING CHANGES
- Update Orleans v3.2
- Update Redis v2.1.58
- Update MessagePack v2.1.143

## [0.6.0](https://github.com/jonathansant/orleans.persistence.redis/compare/0.5.0...0.6.0) (2019-10-25)

### Features

- Update Orleans v3.0 

### BREAKING CHANGES
- Update Orleans v3.0

## [0.5.0](https://github.com/jonathansant/orleans.persistence.redis/compare/0.4.0...0.5.0) (2019-09-26)

### BUG FIXES

- **pubsub:**  add OrleansJsonSerializer settings to json converter

### BREAKING CHANGES

- `IHumanReadableSerializer` add type parameter to the Deserialize

## [0.4.0](https://github.com/jonathansant/orleans.persistence.redis/compare/0.3.1...0.4.0) (2019-06-20)

### Features

- **builder:** add generic host support
- **builder:** move namespace so `Orleans.Hosting`

### BREAKING CHANGES

- `RedisStorageOptionsBuilder` has been renamed to `RedisStorageSiloHostBuilderOptionsBuilder` (for non generic host)

## [0.3.1](https://github.com/jonathansant/orleans.persistence.redis/compare/0.3.0...0.3.1) (2019-05-03)

### Features

- `Ssl`: add Ssl support.

## [0.3.0](https://github.com/jonathansant/orleans.persistence.redis/compare/0.2.2...0.3.0) (2019-04-02)

### Features

- `RedisStorageOptionsBuilder`: add options builder.
