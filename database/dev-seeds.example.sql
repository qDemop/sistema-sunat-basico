-- LOCAL DEVELOPMENT ONLY.
-- UNSAFE FOR PRODUCTION: this file creates a known smoke-test user with a
-- documented password. Do not execute this script in production, staging, QA,
-- shared demos, or any database that contains real data.
--
-- This file is intentionally NOT part of the normal database bootstrap order.
-- Execute it manually only when you need a local Authentication smoke-test user.
--
-- Local/dev-only credentials created by this script:
--   username: devadmin
--   password: ChangeMe.DevOnly.2026!
--
-- The password hash below is a BCrypt hash for the local/dev-only password
-- shown above. Never reuse this password or hash outside local smoke testing.

BEGIN;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM "identity".rol
        WHERE nombre = 'Administrador Sistema'
    ) THEN
        RAISE EXCEPTION 'Required role "Administrador Sistema" is missing. Run the canonical base seeds before this dev-only seed example.';
    END IF;
END $$;

INSERT INTO "identity".usuario AS usuario (
    username,
    password_hash,
    nombre_completo,
    id_rol
)
SELECT
    'devadmin',
    '$2a$11$MEkjgEEGP78iiGCEkrlycOLj6lZEe/9AMfC9DP2DE/.eG71ENATpW',
    'Local Dev Administrator',
    rol.id_rol
FROM "identity".rol AS rol
WHERE rol.nombre = 'Administrador Sistema'
ON CONFLICT (username) DO UPDATE
SET password_hash = EXCLUDED.password_hash,
    nombre_completo = EXCLUDED.nombre_completo,
    id_rol = EXCLUDED.id_rol,
    activo = TRUE,
    intentos_fallidos = 0,
    bloqueado_hasta = NULL;

COMMIT;
