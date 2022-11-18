from liquidctl.cli import *     # noqa
from liquidctl.cli import (
    _PARSE_ARG, _FILTER_OPTIONS, _VALUE_FORMATS, _LOGGER,
    _list_devices_objs, _list_devices_human, _dev_status_obj, _print_dev_status,
    _device_set_color, _device_set_speed,
    _make_opts, _log_requirements, _ErrorAcc
)
from liquidctl import cli, __version__
import json


# Modified from liquidctl.cli to emulate cli interface
def emulate_cli(arg: str):
    args = docopt(cli.__doc__, argv=arg.split(" "))

    if args['--version']:
        print(f'liquidctl v{__version__} ({platform.platform()})')
        sys.exit(0)

    if args['--debug']:
        args['--verbose'] = True
        log_fmt = '%(log_color)s[%(levelname)s] (%(module)s) (%(funcName)s): %(message)s'
        log_level = logging.DEBUG
    elif args['--verbose']:
        log_fmt = '%(log_color)s%(levelname)s: %(message)s'
        log_level = logging.INFO
    else:
        log_fmt = '%(log_color)s%(levelname)s: %(message)s'
        log_level = logging.WARNING
        sys.tracebacklimit = 0

    if sys.platform == 'win32':
        log_colors = {
            'DEBUG': f'bold_blue',
            'INFO': f'bold_purple',
            'WARNING': 'yellow,bold',
            'ERROR': 'red,bold',
            'CRITICAL': 'red,bold,bg_white',
        }
    else:
        log_colors = {
            'DEBUG': f'blue',
            'INFO': f'purple',
            'WARNING': 'yellow,bold',
            'ERROR': 'red,bold',
            'CRITICAL': 'red,bold,bg_white',
        }

    log_fmtter = colorlog.TTYColoredFormatter(fmt=log_fmt, stream=sys.stderr,
                                              log_colors=log_colors)

    log_handler = logging.StreamHandler()
    log_handler.setFormatter(log_fmtter)
    logging.basicConfig(level=log_level, handlers=[log_handler])

    _LOGGER.debug('liquidctl: %s', __version__)
    _LOGGER.debug('platform: %s', platform.platform())
    _log_requirements()

    if __name__ == '__main__':
        _LOGGER.warning('python -m liquidctl.cli is deprecated, prefer python -m liquidctl')

    errors = _ErrorAcc()

    # unlike humans, machines want to know everything; imply verbose everywhere
    # other than when setting default logging level and format (which are
    # inherently for human consumption)
    if args['--json']:
        args['--verbose'] = True

    opts = _make_opts(args)
    opts['_internal_called_from_cli'] = True  # FOR INTERNAL USE ONLY, DO NOT REPLICATE ELSEWHERE
    filter_count = sum(1 for opt in opts if opt in _FILTER_OPTIONS)
    device_id = None

    if not args['--device']:
        selected = list(find_liquidctl_devices(**opts))
    else:
        _LOGGER.warning('-d/--device is deprecated, prefer --match or other selection options')
        device_id = int(args['--device'])
        no_filters = {opt: val for opt, val in opts.items() if opt not in _FILTER_OPTIONS}
        compat = list(find_liquidctl_devices(**no_filters))
        if device_id < 0 or device_id >= len(compat):
            errors.log('device index out of bounds')
            return errors.exit_code()
        if filter_count:
            # check that --device matches other filter criteria
            matched_devs = [dev.device for dev in find_liquidctl_devices(**opts)]
            if compat[device_id].device not in matched_devs:
                errors.log('device index does not match remaining selection criteria')
                return errors.exit_code()
            _LOGGER.warning('mixing --device <id> with other filters is not recommended; '
                            'to disambiguate between results prefer --pick <result>')
        selected = [compat[device_id]]

    if args['list']:
        if args['--json']:
            objs = _list_devices_objs(selected)
            print(json.dumps(objs, ensure_ascii=(os.getenv('LANG', None) == 'C')))
        else:
            _list_devices_human(selected, using_filters=bool(filter_count),
                                device_id=device_id, json=json, **opts)
        return

    if len(selected) > 1 and not (args['status'] or args['all']):
        errors.log('too many devices, filter or select one (see: liquidctl --help)')
        return errors.exit_code()
    elif len(selected) == 0:
        errors.log('no device matches available drivers and selection criteria')
        return errors.exit_code()

    # for json
    obj_buf = []

    for dev in selected:
        _LOGGER.debug('device: %s', dev.description)
        try:
            with dev.connect(**opts):
                if args['initialize']:
                    status = dev.initialize(**opts)
                    if args['--json']:
                        obj_buf.append(_dev_status_obj(dev, status))
                    else:
                        _print_dev_status(dev, status)
                elif args['status']:
                    status = dev.get_status(**opts)
                    if args['--json']:
                        obj_buf.append(_dev_status_obj(dev, status))
                    else:
                        _print_dev_status(dev, status)
                elif args['set'] and args['speed']:
                    _device_set_speed(dev, args, **opts)
                elif args['set'] and args['color']:
                    _device_set_color(dev, args, **opts)
                else:
                    assert False, 'unreachable'
        except OSError as err:
            # each backend API returns a different subtype of OSError (OSError,
            # usb.core.USBError or PermissionError) for permission issues
            if err.errno in [errno.EACCES, errno.EPERM]:
                errors.log(f'insufficient permissions to access {dev.description}', err=err)
            elif err.args == ('open failed', ):
                errors.log(
                    f'could not open {dev.description}, possibly due to insufficient permissions',
                    err=err
                )
            else:
                errors.log(f'unexpected OS error with {dev.description}', err=err, show_err=True)
        except NotSupportedByDevice as err:
            errors.log(f'operation not supported by {dev.description}', err=err)
        except NotSupportedByDriver as err:
            errors.log(f'operation not supported by driver for {dev.description}', err=err)
        except UnsafeFeaturesNotEnabled as err:
            features = ','.join(err.args)
            errors.log(f'missing --unsafe features for {dev.description}: {features!r}', err=err)
        except Exception as err:
            errors.log(f'unexpected error with {dev.description}', err=err, show_err=True)

    if errors.is_empty() and args['--json']:
        # use __str__ for values that cannot be directly serialized to JSON
        # (e.g. enums)
        return json.dumps(obj_buf, ensure_ascii=(os.getenv('LANG', None) == 'C'),
                         default=lambda x: str(x))

    return json.dumps([])

if __name__ == "__main__":
    while True:
        print(emulate_cli(input(">").strip("\n")))
