import sys

if __name__ == "__main__":
    if (len(sys.argv) != 3):
        exit()

    prefab = sys.argv[1]
    f = open(prefab, mode='r', encoding='utf-8')
    lst = []
    cnt = 0;
    for line in f.readlines():
        if cnt > 0:
            if cnt == 1:
                lst.append(" :> \n")
            lst.append(line)
            cnt = cnt - 1
        elif (line.startswith("(Filename")): #new component
            lst.append(line)
            cnt = 3;
    f.close();

    writeFile = sys.argv[2]
    f = open(writeFile, mode='w', encoding='utf-8')
    for a in lst:
        if a.strip():
            f.write(str(a))
    f.flush()
    f.close()
