#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
    CREATE DATABASE catalog_db;
    CREATE DATABASE identity_db;
    CREATE DATABASE orders_db;
EOSQL
