-- Platform catalog + optional legacy dev database (utf8mb4).
CREATE DATABASE IF NOT EXISTS schoolsaas_platform
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

CREATE DATABASE IF NOT EXISTS schoolsaas_dev
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;
