-- Sequence: public.event_a_seq

-- DROP SEQUENCE public.event_a_seq;

CREATE SEQUENCE public.event_a_seq
  INCREMENT 1
  MINVALUE 1
  MAXVALUE 9223372036854775807
  START 1
  CACHE 1;
ALTER TABLE public.event_a_seq
  OWNER TO postgres;

-- Table: public.event_a

-- DROP TABLE public.event_a;

CREATE TABLE public.event_a
(
  store_version bigint NOT NULL DEFAULT nextval('event_a_store_version_seq'::regclass),
  origin_user_id uuid,
  origin_system_id uuid,
  aggregate_id uuid NOT NULL,
  aggregate_version bigint NOT NULL,
  aggregate_type_id character varying NOT NULL,
  aggregate_type_unique_key character varying,
  event_payload_type_id character varying NOT NULL,
  event_timestamp time without time zone NOT NULL,
  payload json,
  CONSTRAINT store_version_pkey PRIMARY KEY (store_version),
  CONSTRAINT aggregate_id_aggregate_version_key UNIQUE (aggregate_id, aggregate_version)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public.event_a
  OWNER TO postgres;

-- Index: public.event_timestamp_idx

-- DROP INDEX public.event_timestamp_idx;

CREATE INDEX event_timestamp_idx
  ON public.event_a
  USING btree
  (event_timestamp);

